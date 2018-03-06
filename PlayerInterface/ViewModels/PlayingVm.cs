using PlayerCore;
using PlayerInterface.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;

namespace PlayerInterface.ViewModels {
    public class PlayingVm : NotifyPropertyChanged {
        private const string ImgSourcePlay = "pack://application:,,,/res/img/Play.png";
        private const string ImgSourcePause = "pack://application:,,,/res/img/Pause.png";

        //Slider should not all the way to end of end of track, track should end 'naturaly'
        private int ElapsedTrackEndBufferMs = 500;

        private Timer UpdateTimer;

        public SongPlayer SongPlayer { get; }

        public TransitionManager TransitionMngr { get; }

        public IBaseCommand SwitchCommand { get; }

        public string SwitchButtonImgSource =>
            SongPlayer?.PlayerState == PlayerState.Playing || TransitionMngr.IsTransitioning ? ImgSourcePause : ImgSourcePlay;

        public string ElapsedStr => FormatHelper.FormatTimeSpan(SongPlayer.Elapsed);

        public double ElapsedFraction {
            get { return SongPlayer.Elapsed.TotalMilliseconds / (SongPlayer.TrackLength.TotalMilliseconds - ElapsedTrackEndBufferMs); }
            set {
                if (value >= 0 && value <= 1) {
                    var miliseconds = (SongPlayer.TrackLength.TotalMilliseconds - ElapsedTrackEndBufferMs) * value;
                    var newTime = TimeSpan.FromMilliseconds(miliseconds);
                    SongPlayer.Elapsed = newTime;
                }
            }
        }

        public bool EnableChangeElapsed => SongPlayer?.CurrentSong != null && SongPlayer?.PlayerState != PlayerState.Stopped;

        public Brush ElapsedColor => SongPlayer?.PlayerState == PlayerState.Playing ? System.Windows.SystemColors.HighlightBrush :
            (SongPlayer?.PlayerState == PlayerState.Paused ? Brushes.OrangeRed : Brushes.Transparent);

        public float Volume {
            get { return (float)Math.Round(SongPlayer.Volume, 2); }
            set {
                SongPlayer.Volume = value;
                RaisePropertyChanged();
            }
        }

        public PlayingVm(SongPlayer player, TransitionManager transitionMngr) {
            SongPlayer = player;
            TransitionMngr = transitionMngr;

            SongPlayer.SongChanged += (s, a) => RaisePropertiesChanged(nameof(ElapsedFraction), nameof(ElapsedStr));
            SongPlayer.PlayingStopped += (s, a) => RaisePropertiesChanged(nameof(SwitchButtonImgSource), nameof(EnableChangeElapsed));
            SongPlayer.PlaybackStateChanged += (s, a) => RaisePropertiesChanged(nameof(SwitchButtonImgSource), nameof(EnableChangeElapsed), nameof(ElapsedColor));
            SongPlayer.VolumeChanged += (s, a) => RaisePropertyChanged(nameof(Volume));
            TransitionMngr.TransitionChanged += (s, a) => RaisePropertiesChanged(nameof(SwitchButtonImgSource), nameof(EnableChangeElapsed));

            UpdateTimer = new Timer() {
                AutoReset = true,
                Enabled = true,
                Interval = 500
            };
            UpdateTimer.Elapsed += (s, a) => RaisePropertiesChanged(nameof(ElapsedStr), nameof(ElapsedFraction));

            var sc = new RelayCommand(
                execute: _ => {
                    if (TransitionMngr.IsTransitioning) {
                        TransitionMngr.CancelTransition();
                    } else if (SongPlayer.PlayerState == PlayerState.Playing) {
                        SongPlayer.PlayerState = PlayerState.Paused;
                    } else {
                        SongPlayer.PlayerState = PlayerState.Playing;
                    }
                },
                canExecute: _ => SongPlayer.CurrentSong != null
            );
            SongPlayer.SongChanged += (s, a) => sc.RaiseCanExecuteChanged();
            SwitchCommand = sc;
        }
    }
}
