using NAudio.Wave;
using PlayerCore;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class FullPlayerViewModel : SmallPlayerViewModel {
        private Timer UpdateTimer;

        //Slider should not all the way to end of end of track, track should end 'naturaly'
        private int SliderTrackEndBufferMs = 500;

        public string ElapsedStr => FormatHelper.FormatTimeSpan(SongPlayer.Elapsed);

        public string TrackLengthStr => FormatHelper.FormatTimeSpan(SongPlayer.TrackLength);

        public string StatusText => $"{SongPlayer.CurrentSong.Title} - {SongPlayer.CurrentSong.Artist}";

        public double ElapsedFraction {
            get { return SongPlayer.Elapsed.TotalMilliseconds / (SongPlayer.TrackLength.TotalMilliseconds - SliderTrackEndBufferMs); }
            set {
                if(value >= 0 && value <= 1) {
                    var miliseconds = (SongPlayer.TrackLength.TotalMilliseconds - SliderTrackEndBufferMs) * value;
                    var newTime = TimeSpan.FromMilliseconds(miliseconds);
                    SongPlayer.Elapsed = newTime;
                }
            }
        }

        public ICommand PlaySongCommand;

        public SongViewModel CurrentFocusItem {
            get; set;
        }

        public ObservableCollection<SongViewModel> PlaylistItems {
            get; private set;
        }

        public FullPlayerViewModel(SongPlayer player, Playlist playlist) : base(player, playlist) {
            PlaySongCommand = new RelayCommand((s) => {
                if(s as Song != null) {
                    Playlist.PlayFirstMatch((Song)s);
                }
            }, (s) => {
                return SongPlayer != null && s as Song != null;
            });

            UpdateTimer = new Timer() {
                AutoReset = true,
                Enabled = true,
                Interval = 1000
            };

            PlaylistItems = new ObservableCollection<SongViewModel>(Playlist.CurrentList.Select(s => new SongViewModel(s)));

            SongPlayer.SongChanged += SongPlayer_SongChanged;
            Playlist.ListChanged += Playlist_ListChanged;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private void Playlist_ListChanged(object sender, EventArgs e) {
            PlaylistItems.Clear();
            foreach(var add in Playlist.CurrentList.Select(s => new SongViewModel(s))) {
                PlaylistItems.Add(add);
            }
        }

        private void SongPlayer_SongChanged(object sender, EventArgs e) {
            var curPlaying = PlaylistItems.FirstOrDefault(svm => svm.Playing);
            if(curPlaying != null)
                curPlaying.Playing = false;

            var newPlaying = PlaylistItems.FirstOrDefault(svm => svm.Song == SongPlayer.CurrentSong);
            if(newPlaying != null) {
                newPlaying.Playing = true;
                CurrentFocusItem = newPlaying;
                RaisePropertiesChanged(nameof(CurrentFocusItem));
            }

            RaisePropertiesChanged(nameof(ElapsedStr), nameof(ElapsedFraction), nameof(TrackLengthStr), nameof(StatusText));
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e) {
            RaisePropertiesChanged(nameof(ElapsedStr), nameof(ElapsedFraction));
        }
    }
}