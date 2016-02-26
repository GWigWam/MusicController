using NAudio.Wave;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PlayerCore {

    public class SongPlayer {
        private Song currentSong;
        private AudioFileReader File;
        private IWavePlayer Player;
        private bool PlayedToEnd;

        public Song CurrentSong {
            get { return currentSong; }
            set {
                currentSong = value;
                if(currentSong != null) {
                    bool wasPlaying = PlayedToEnd || PlayerState == PlayerState.Playing;
                    LoadSong(currentSong);
                    if(wasPlaying) {
                        Player.Play();
                    }
                    PlayedToEnd = false;
                }
                SongChanged?.Invoke(this, new EventArgs());
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
                if(StateConverter.Convert(Player?.PlaybackState) != value) {
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

        private float volume;

        public float Volume {
            get { return volume; }
            set {
                if(volume != value && value >= 0 && value <= 1) {
                    volume = value;
                    if(File != null)
                        File.Volume = value;
                }
            }
        }

        public event EventHandler SongEnded;

        public event EventHandler SongChanged;

        public event EventHandler<PlaybackStateChangedEventArgs> PlaybackStateChanged;

        public SongPlayer(float volume = 1) {
            Volume = volume;
        }

        public void Stop() {
            CurrentSong = null;
            if(Player != null) {
                Player.Stop();
                Player.Dispose();
                Player = null;
            }
        }

        private void LoadSong(Song song) {
            if(song != null) {
                try {
                    if(Player != null) {
                        // Dispose the player BEFORE disposing the file
                        // Make sure event PlaybackStopped isn't fired
                        Player.PlaybackStopped -= Player_PlaybackStopped;
                        Player.Dispose();
                    }
                    if(File != null) {
                        File.Dispose();
                    }

                    File = new AudioFileReader(song.FilePath) { Volume = Volume };

                    // 'WaveOutEvent' should be less bound to UI than 'WaveOut', they are interchangable (both IWavePlayer)
                    // High latency makes player unresponsive when changing 'Elapsed' time, and 'Elapsed' property is less accurate
                    // When playing 1 buffer is sent to audio-card, the others are in memory waiting to be sent, thus many buffers means more RAM usage
                    // Amount of music in mem = (Latency * NrOfBuffers) because latency == size per buffer
                    // See: https://github.com/naudio/NAudio/wiki/Understanding-Output-Devices
                    Player = new WaveOutEvent() {
                        DesiredLatency = 400, // In ms, Default = 300
                        NumberOfBuffers = 10 // Default = 2
                    };

                    Player.Init(File);
                    Player.PlaybackStopped += Player_PlaybackStopped;
                } catch(Exception) {
                    Stop();
                    SongEnded?.Invoke(this, new EventArgs());
                    //TODO: Throw userfriendly exception
                }
            }
        }

        private void Player_PlaybackStopped(object sender, StoppedEventArgs e) {
            PlayedToEnd = true;
            SongEnded?.Invoke(this, new EventArgs());
        }
    }
}