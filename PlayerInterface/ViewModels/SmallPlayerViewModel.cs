using NAudio.Wave;
using PlayerCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class SmallPlayerViewModel : INotifyPropertyChanged {
        private const string ImgSourcePlay = "res/img/Play.png";
        private const string ImgSourcePause = "res/img/Pause.png";

        protected SongPlayer songPlayer;

        public SongPlayer SongPlayer {
            get {
                return songPlayer;
            }
            set {
                songPlayer = value;
                songPlayer.SongEnded += SongPlayer_SongEnded;
            }
        }

        public ICommand SwitchCommand {
            get; private set;
        }

        public string SwitchButtonImgSource => SongPlayer?.PlayerState == PlaybackState.Playing ? ImgSourcePause : ImgSourcePlay;

        public float Volume {
            get { return SongPlayer.Volume; }
            set { if(value >= 0 && value <= 1) SongPlayer.Volume = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SmallPlayerViewModel(SongPlayer player) {
            SongPlayer = player;

            SwitchCommand = new RelayCommand((o) => {
                if(player.PlayerState == PlaybackState.Playing) {
                    player.PlayerState = PlaybackState.Paused;
                } else {
                    player.PlayerState = PlaybackState.Playing;
                }
            });

            SongPlayer.PlaybackStateChanged += PlaybackStateChanged;
        }

        public void StartPlaying() {
            SongPlayer.StartPlaying();
        }

        public void Stop() {
            SongPlayer.Stop();
        }

        private void PlaybackStateChanged(object sender, PlaybackStateChangedEventArgs e) {
            RaisePropertiesChanged("SwitchButtonImgSource");
        }

        protected string FormatTimeSpan(TimeSpan ts) {
            var format = Math.Floor(ts.TotalHours) > 0 ? @"h\:m\:ss" : @"m\:ss";
            return ts.ToString(format);
        }

        protected void RaisePropertiesChanged(params string[] propertyNames) {
            foreach(var propName in propertyNames) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        protected void SongPlayer_SongEnded(object sender, Song e) {
            //todo
        }
    }
}