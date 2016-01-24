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

        public Song CurrentSong {
            get { return currentSong; }
            set {
                currentSong = value;
                LoadFile();
                if(Player?.PlaybackState == PlaybackState.Playing) {
                    StartPlaying();
                }
            }
        }

        public TimeSpan Elapsed {
            set {
                if(File != null)
                    File.CurrentTime = value;
            }
            get {
                if(File == null || PlayerState != PlaybackState.Playing) {
                    return TimeSpan.Zero;
                } else {
                    return File.CurrentTime;
                }
            }
        }

        public PlaybackState PlayerState {
            get {
                return Player?.PlaybackState ?? PlaybackState.Stopped;
            }
            set {
                var oldValue = Player?.PlaybackState;
                if(value == PlaybackState.Playing) {
                    if(Player?.PlaybackState == PlaybackState.Paused) {
                        Player.Play();
                    } else {
                        StartPlaying();
                    }
                } else if(value == PlaybackState.Paused) {
                    Player.Pause();
                } else if(value == PlaybackState.Stopped) {
                    Stop();
                }

                if(oldValue != value)
                    PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(oldValue, value));
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
                volume = value;
                if(File != null)
                    File.Volume = value;
            }
        }

        public event EventHandler<Song> SongEnded;

        public event EventHandler<PlaybackStateChangedEventArgs> PlaybackStateChanged;

        public SongPlayer(float volume = 1) {
            Volume = volume;
        }

        public void StartPlaying() {
            if(CurrentSong != null) {
                if(File == null) {
                    LoadFile();
                }
                if(Player == null) {
                    Player = new WaveOut();
                    try {
                        Player.Init(File);
                        Player.PlaybackStopped += (s, a) => SongEnded?.Invoke(this, CurrentSong);
                    } catch(Exception) {
                        Stop();
                        SongEnded?.Invoke(this, CurrentSong);
                        //TODO: Throw userfriendly exception
                    }
                }
                Player.Play();
            }
        }

        public void Stop() {
            if(Player != null) {
                Player.Stop();
                Player.Dispose();
                Player = null;
            }
        }

        private void LoadFile() {
            File = new AudioFileReader(CurrentSong.FilePath) { Volume = Volume };
        }
    }
}