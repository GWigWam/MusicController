using NAudio.Wave;
using PlayerCore.PlayerEventArgs;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PlayerCore {

    public class SongPlayer : IDisposable {
        public static readonly string[] SupportedExtensions = new string[] { ".mp3", ".flac" };

        private Song currentSong;
        private IWavePlayer Player;
        private bool PlayedToEnd;

        private WaveStream File;
        private AudioFileReader AudioFileReader => File as AudioFileReader;

        public Song CurrentSong {
            get { return currentSong; }
            set {
                var oldSong = currentSong;
                currentSong = value;
                if(currentSong != null) {
                    bool wasPlaying = PlayerState == PlayerState.Playing;
                    LoadSong(currentSong);
                    if(wasPlaying) {
                        Player.Play();
                    } else if(PlayedToEnd) {
                        PlayerState = PlayerState.Playing;
                    }
                    PlayedToEnd = false;
                }
                SongChanged?.Invoke(this, new SongChangedEventArgs(oldSong, currentSong));
            }
        }

        public TimeSpan Elapsed {
            set {
                if(File != null)
                    File.CurrentTime = value;
            }
            get {
                if(File == null || PlayerState == PlayerState.Stopped) {
                    return TimeSpan.Zero;
                } else {
                    return File.CurrentTime;
                }
            }
        }

        public PlayerState PlayerState {
            get {
                return StateConverter.Convert(Player?.PlaybackState ?? PlaybackState.Stopped);
            }
            set {
                if(StateConverter.Convert(Player?.PlaybackState) != value && Player != null) {
                    var oldValue = StateConverter.Convert(Player?.PlaybackState);
                    if(value == PlayerState.Playing) {
                        Player.Play();
                    } else if(value == PlayerState.Paused) {
                        Player.Pause();
                    } else if(value == PlayerState.Stopped) {
                        Stop();
                    }

                    if(oldValue != value)
                        PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(oldValue, value));
                }
            }
        }

        public void TogglePause(bool startIfStopped) {
            if(PlayerState == PlayerState.Paused) {
                PlayerState = PlayerState.Playing;
            } else if(PlayerState == PlayerState.Playing) {
                PlayerState = PlayerState.Paused;
            } else if(PlayerState == PlayerState.Stopped && startIfStopped && CurrentSong != null) {
                PlayerState = PlayerState.Playing;
            }
        }

        public TimeSpan TrackLength {
            get {
                return File?.TotalTime ?? TimeSpan.Zero;
            }
        }

        private float _Volume;
        public float Volume {
            get { return _Volume; }
            set {
                if(_Volume != value && value >= 0) {
                    _Volume = value;
                    if(AudioFileReader != null) {
                        AudioFileReader.Volume = _Volume;
                    }
                    VolumeChanged?.Invoke(this, _Volume);
                }
            }
        }

        public event EventHandler<PlayingStoppedEventArgs> PlayingStopped;

        public event EventHandler<SongChangedEventArgs> SongChanged;

        public event EventHandler<PlaybackStateChangedEventArgs> PlaybackStateChanged;

        public event EventHandler<float> VolumeChanged;

        public SongPlayer(float volume = 1) {
            Volume = volume;
        }

        public void Stop() {
            CurrentSong = null;

            if(Player != null) {
                Player.Stop();
            }

            DisposeInternal(Player, File);
            Player = null;
            File = null;

            if(PlayerState != PlayerState.Stopped) {
                PlayerState = PlayerState.Stopped;
            }
            PlayedToEnd = false;
        }

        private void DisposeInternal(IWavePlayer player, WaveStream file) {
            new TaskFactory().StartNew(() => {
                if(player != null) {
                    player.Dispose();
                }

                if(file != null) {
                    // Given that the NAudio framework gives no access to inner systems the best way to
                    // assure the player has been disposed before disposing the file is to wait a while
                    Task.Delay(1000).Wait();
                    file.Dispose();
                }
            });
        }

        private void LoadSong(Song song) {
            if(song != null) {
                try {
                    if(Player != null) {
                        // Dispose the player BEFORE disposing the file
                        // Make sure event PlaybackStopped isn't fired
                        Player.PlaybackStopped -= Player_PlaybackStopped;
                        Player.Dispose();
                        Player = null;
                    }
                    if(File != null) {
                        File.Dispose();
                        File = null;
                    }

                    File = new AudioFileReader(song.FilePath) { Volume = Volume };

                    // 'WaveOutEvent' should be less bound to UI than 'WaveOut', they are interchangable (both IWavePlayer)
                    // High latency makes player unresponsive when changing 'Elapsed' time, and 'Elapsed' property is less accurate
                    // When playing 1 buffer is sent to audio-card, the others are in memory waiting to be sent, thus many buffers means more RAM usage
                    // Amount of music in mem = (Latency * NrOfBuffers) because latency == size per buffer
                    // See: https://github.com/naudio/NAudio/wiki/Understanding-Output-Devices
                    Player = new WaveOutEvent() {
                        DesiredLatency = 300, // In ms, Default = 300
                        NumberOfBuffers = 3, // Default = 2
                        DeviceNumber = -1 // 1- for default device, 0 for first device
                    };

                    Player.Init(File);
                    Player.PlaybackStopped += Player_PlaybackStopped;
                } catch(Exception e) {
                    Stop();
                    PlayingStopped?.Invoke(this, new PlayingStoppedEventArgs(false, e));
                    //TODO: Throw userfriendly exception
                    if(e is FileNotFoundException || e is InvalidDataException) {
                        throw new SongLoadFailedException(song, e);
                    } else {
                        throw e;
                    }
                }
            }
        }

        private void Player_PlaybackStopped(object sender, StoppedEventArgs e) {
            PlayingStoppedEventArgs stoppedEventArgs = null;
            if(e.Exception == null) {
                PlayedToEnd = true;
                stoppedEventArgs = new PlayingStoppedEventArgs(true);
            } else {
                stoppedEventArgs = new PlayingStoppedEventArgs(false, e.Exception);
            }
            PlayingStopped?.Invoke(this, stoppedEventArgs);
        }

        public void Dispose() {
            Stop(); //Already handles disposing of 'File' and 'Player' objects
        }
    }

    public class SongLoadFailedException : Exception {
        private const string LoadFailMessage = "Loading song failed";

        public Song Song { get; private set; }

        public SongLoadFailedException(Song song, Exception innerException) : base(LoadFailMessage, innerException) {
            Song = song;
        }

        public SongLoadFailedException(Song song) : base(LoadFailMessage) {
            Song = song;
        }
    }
}