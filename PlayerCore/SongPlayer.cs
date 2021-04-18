using NAudio.Wave;
using PlayerCore.PlayerEventArgs;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore
{
    public class SongPlayer
    {
        public static readonly string[] SupportedExtensions = new string[] { ".mp3", ".flac" };

        public event EventHandler<PlayingStoppedEventArgs>? PlayingStopped;
        public event EventHandler<SongChangedEventArgs>? SongChanged;
        public event EventHandler? PlaybackStateChanged;

        private IWavePlayer? Player { get; set; }
        private AudioFileReader? File { get; set; }

        private Song? _CurrentSong;
        public Song? CurrentSong {
            get => _CurrentSong;
            set {
                var oldSong = _CurrentSong;
                _CurrentSong = value;

                if(_CurrentSong != null)
                {
                    LoadSong(_CurrentSong, startPlaying: IsPlaying);
                }

                SongChanged?.Invoke(this, new SongChangedEventArgs(oldSong, _CurrentSong));
            }
        }

        public TimeSpan Elapsed {
            get => File != null && (IsPlaying || IsPaused) ? File.CurrentTime : TimeSpan.Zero;
            set {
                if(File != null)
                {
                    File.CurrentTime = value;
                }
            }
        }

        public bool IsPlaying => Player?.PlaybackState == PlaybackState.Playing;
        public bool IsPaused => Player?.PlaybackState == PlaybackState.Paused;

        public TimeSpan TrackLength => File?.TotalTime ?? TimeSpan.Zero;

        public float Volume {
            set {
                if(value >= 0 && File != null)
                {
                    File.Volume = value;
                }
            }
        }

        public void Play()
        {
            Player?.Play();
            PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            Player?.Pause();
            PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            CurrentSong = null;
            Player?.Stop();
            Player?.Dispose();
            File?.Dispose();
            Player = null;
            File = null;
            PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LoadSong(Song song, bool startPlaying)
        {
            try
            {
                if(Player != null)
                {
                    // Dispose the player BEFORE disposing the file
                    // Make sure event PlaybackStopped isn't fired
                    Player.PlaybackStopped -= Player_PlaybackStopped;
                    Player.Dispose();
                }
                File?.Dispose();

                File = new AudioFileReader(song.Path);

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

                if(startPlaying)
                {
                    Player.Play();
                }
            }
            catch(Exception e)
            {
                Stop();
                PlayingStopped?.Invoke(this, new PlayingStoppedEventArgs(false, e));

                if(e is FileNotFoundException || e is InvalidDataException)
                {
                    throw new SongLoadFailedException(song, e);
                }
                else
                {
                    throw;
                }
            }
        }

        private void Player_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if(e.Exception == null)
            {
                PlayingStopped?.Invoke(this, new PlayingStoppedEventArgs(true));
            }
            else
            {
                PlayingStopped?.Invoke(this, new PlayingStoppedEventArgs(false, e.Exception));
            }
        }
    }

    public class SongLoadFailedException : Exception
    {
        private const string LoadFailMessage = "Loading song failed";

        public Song Song { get; private set; }

        public SongLoadFailedException(Song song, Exception innerException) : base(LoadFailMessage, innerException)
        {
            Song = song;
        }

        public SongLoadFailedException(Song song) : base(LoadFailMessage)
        {
            Song = song;
        }
    }
}
