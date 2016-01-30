using NAudio.Wave;
using PlayerCore;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class SmallPlayerViewModel : INotifyPropertyChanged {
        private const string ImgSourcePlay = "pack://application:,,,/res/img/Play.png";
        private const string ImgSourcePause = "pack://application:,,,/res/img/Pause.png";

        public SongPlayer SongPlayer {
            get; private set;
        }

        public Playlist Playlist {
            get; private set;
        }

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

        public string SwitchButtonImgSource => SongPlayer?.PlayerState == PlaybackState.Playing ? ImgSourcePause : ImgSourcePlay;

        public float Volume {
            get { return SongPlayer.Volume; }
            set { if(value >= 0 && value <= 1) SongPlayer.Volume = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SmallPlayerViewModel(SongPlayer player, Playlist playlist) {
            SongPlayer = player;
            Playlist = playlist;

            SetupCommands();

            SongPlayer.SongEnded += SongPlayer_SongEnded;
            SongPlayer.PlaybackStateChanged += PlaybackStateChanged;
        }

        private void SetupCommands() {
            SwitchCommand = new RelayCommand((o) => {
                if(SongPlayer.PlayerState == PlaybackState.Playing) {
                    SongPlayer.PlayerState = PlaybackState.Paused;
                } else {
                    SongPlayer.PlayerState = PlaybackState.Playing;
                }
            }, (o) => {
                return SongPlayer.CurrentSong != null;
            });

            NextCommand = new RelayCommand((o) => {
                Playlist.CurrentSongIndex++;
            }, (o) => Playlist.HasNext);

            PreviousCommand = new RelayCommand((o) => {
                Playlist.CurrentSongIndex--;
            }, (o) => Playlist.HasPrevious);

            ShuffleCommand = new RelayCommand((o) => Playlist.Shuffle(), (o) => Playlist != null);
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