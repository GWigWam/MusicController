using PlayerCore;
using PlayerCore.PlayerEventArgs;
using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Commands;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;

namespace PlayerInterface.ViewModels {

    public class SmallPlayerViewModel : NotifyPropertyChanged {
        private const string ImgSourcePlay = "pack://application:,,,/res/img/Play.png";
        private const string ImgSourcePause = "pack://application:,,,/res/img/Pause.png";
        private const int PreviousRestartMinTimeMs = 5000;

        //Slider should not all the way to end of end of track, track should end 'naturaly'
        private int ElapsedTrackEndBufferMs = 500;

        private Timer UpdateTimer;

        public AppSettings Settings { get; }

        public SongPlayer SongPlayer { get; }

        public Playlist Playlist { get; }

        public TransitionManager TransitionMngr { get; }

        public ICommand SwitchCommand {
            get; private set;
        }

        public ICommand NextCommand {
            get; private set;
        }

        public ICommand PreviousCommand {
            get; private set;
        }

        public ICommand ShuffleCommand {
            get; private set;
        }

        public string SwitchButtonImgSource => SongPlayer?.PlayerState == PlayerState.Playing || TransitionMngr.IsTransitioning ? ImgSourcePause : ImgSourcePlay;

        public float Volume {
            get { return SongPlayer.Volume; }
            set {
                if(value >= 0 && value <= 1) {
                    SongPlayer.Volume = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool EnableChangeElapsed => SongPlayer?.CurrentSong != null && SongPlayer?.PlayerState != PlayerState.Stopped;

        public string ElapsedStr => FormatHelper.FormatTimeSpan(SongPlayer.Elapsed);

        public double ElapsedFraction {
            get { return SongPlayer.Elapsed.TotalMilliseconds / (SongPlayer.TrackLength.TotalMilliseconds - ElapsedTrackEndBufferMs); }
            set {
                if(value >= 0 && value <= 1) {
                    var miliseconds = (SongPlayer.TrackLength.TotalMilliseconds - ElapsedTrackEndBufferMs) * value;
                    var newTime = TimeSpan.FromMilliseconds(miliseconds);
                    SongPlayer.Elapsed = newTime;
                }
            }
        }

        public Brush ElapsedColor => SongPlayer?.PlayerState == PlayerState.Playing ? System.Windows.SystemColors.HighlightBrush :
            (SongPlayer?.PlayerState == PlayerState.Paused ? Brushes.OrangeRed : Brushes.Transparent);

        public SmallPlayerViewModel(AppSettings settings, SongPlayer player, Playlist playlist, TransitionManager transitionMngr) {
            Settings = settings;
            SongPlayer = player;
            Playlist = playlist;

            TransitionMngr = transitionMngr;
            TransitionMngr.TransitionChanged += (s, a) => RaisePropertiesChanged(nameof(SwitchButtonImgSource), nameof(EnableChangeElapsed));

            SetupCommands();

            SongPlayer.SongChanged += (s, a) => RaisePropertiesChanged(nameof(ElapsedFraction), nameof(ElapsedStr));
            SongPlayer.PlayingStopped += Player_PlayingStopped;
            SongPlayer.PlaybackStateChanged += PlaybackStateChanged;
            SongPlayer.VolumeChanged += SongPlayer_VolumeChanged;

            UpdateTimer = new Timer() {
                AutoReset = true,
                Enabled = true,
                Interval = 1000
            };
            UpdateTimer.Elapsed += (s, a) => RaisePropertiesChanged(nameof(ElapsedStr), nameof(ElapsedFraction));
        }

        private void SetupCommands() {
            SwitchCommand = new RelayCommand((o) => {
                if(TransitionMngr.IsTransitioning) {
                    TransitionMngr.CancelTransition();
                } else if(SongPlayer.PlayerState == PlayerState.Playing) {
                    SongPlayer.PlayerState = PlayerState.Paused;
                } else {
                    SongPlayer.PlayerState = PlayerState.Playing;
                }
            }, (o) => {
                return SongPlayer.CurrentSong != null;
            });

            NextCommand = new RelayCommand((o) => {
                Playlist.CurrentSongIndex++;
            }, (o) => Playlist.HasNext);

            PreviousCommand = new RelayCommand((o) => {
                if(SongPlayer.Elapsed.TotalMilliseconds > PreviousRestartMinTimeMs) {
                    SongPlayer.Elapsed = TimeSpan.FromMilliseconds(0);
                } else {
                    Playlist.CurrentSongIndex--;
                }
            }, (o) => Playlist.HasPrevious || SongPlayer.Elapsed.TotalMilliseconds > PreviousRestartMinTimeMs);

            ShuffleCommand = new RelayCommand((o) => Playlist.Shuffle(), (o) => Playlist != null);
        }

        public void Stop() {
            SongPlayer.Stop();
        }

        private void PlaybackStateChanged(object sender, PlaybackStateChangedEventArgs e) {
            RaisePropertiesChanged(nameof(SwitchButtonImgSource), nameof(EnableChangeElapsed), nameof(ElapsedColor));
        }

        private void SongPlayer_VolumeChanged(object sender, float e) {
            RaisePropertyChanged(nameof(Volume));
        }

        protected void Player_PlayingStopped(object sender, PlayingStoppedEventArgs args) {
            RaisePropertiesChanged(nameof(SwitchButtonImgSource), nameof(EnableChangeElapsed));
        }
    }
}