using NAudio.Wave;
using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class FullPlayerViewModel : SmallPlayerViewModel {
        private Timer UpdateTimer;

        //Slider should not all the way to end of end of track, track should end 'naturaly'
        private int SliderTrackEndBufferMs = 500;

        public string ElapsedStr => FormatHelper.FormatTimeSpan(SongPlayer.Elapsed);

        public string TrackLengthStr => FormatHelper.FormatTimeSpan(SongPlayer.TrackLength);

        public string StatusText => $"{SongPlayer?.CurrentSong?.Title} - {SongPlayer?.CurrentSong?.Artist}";

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

        public Visibility ShowDropHint => (Playlist?.Length ?? 0) > 0 ? Visibility.Collapsed : Visibility.Visible;

        public ICommand PlaySongCommand {
            get; private set;
        }

        public ICommand ReverseSortCommand {
            get; private set;
        }

        public ICommand SortByCommand {
            get; private set;
        }

        public ICommand RemoveSongCommand {
            get; private set;
        }

        public ICommand SearchCommand {
            get; private set;
        }

        public ICommand AddFilesCommand {
            get; private set;
        }

        public SongViewModel CurrentFocusItem {
            get; set;
        }

        public ObservableCollection<SongViewModel> PlaylistItems {
            get; private set;
        }

        public string PlaylistStats => $"{Playlist?.Length} - {FormatHelper.FormatTimeSpan(new TimeSpan(Playlist?.Sum(s => s.File.TrackLength.Ticks) ?? 0))}";

        public AppSettingsViewModel SettingsViewModel {
            get;
        }

        public FullPlayerViewModel(AppSettings settings, SongPlayer player, Playlist playlist) : base(settings, player, playlist) {
            SetupCommands();

            SettingsViewModel = new AppSettingsViewModel(Settings);

            PlaylistItems = new ObservableCollection<SongViewModel>(Playlist.Select(s => new SongViewModel(s)));

            SongPlayer.SongChanged += SongPlayer_SongChanged;
            Playlist.ListChanged += Playlist_ListChanged;

            UpdateTimer = new Timer() {
                AutoReset = true,
                Enabled = true,
                Interval = 1000
            };
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private void SetupCommands() {
            PlaySongCommand = new RelayCommand((s) => {
                if(s as Song != null) {
                    Playlist.PlayFirstMatch((Song)s);
                }
            }, (s) => {
                return SongPlayer != null && s as Song != null;
            });

            ReverseSortCommand = new RelayCommand((o) => Playlist.Reverse(), (o) => Playlist != null);

            SortByCommand = new RelayCommand(
                (o) => {
                    var pi = o as PropertyInfo;
                    if(pi.DeclaringType == typeof(Song)) {
                        Playlist.Order((s) => pi.GetValue(s));
                    } else if(pi.DeclaringType == typeof(SongFile)) {
                        Playlist.Order((s) => pi.GetValue(s.File));
                    }
                },
                (o) => o as PropertyInfo != null && (((PropertyInfo)o).DeclaringType == typeof(Song) || ((PropertyInfo)o).DeclaringType == typeof(SongFile))
            );

            RemoveSongCommand = new RelayCommand((o) => {
                Playlist.Remove(((IEnumerable<SongViewModel>)o).Select(svm => svm.Song));
            }, (o) => {
                var songs = o as IEnumerable<SongViewModel>;
                return songs != null && songs.Count() > 0;
            });

            SearchCommand = new RelayCommand((o) => FillPlaylist(o as string));

            AddFilesCommand = new RelayCommand((o) => {
                var files = o as string[];
                if(files != null) {
                    foreach(var add in files) {
                        if(Directory.Exists(add)) {
                            var songs = SongFileReader.ReadFolderFiles(add, "*.mp3" /*TODO get from settings*/);
                            Playlist.AddSongs(songs.Select(sf => new Song(sf)));
                        } else if(File.Exists(add)) {
                            var song = SongFileReader.ReadFile(add);
                            if(song != null) {
                                Playlist.AddSong(new Song(song));
                            }
                        }
                    }
                }
            });
        }

        private void Playlist_ListChanged(object sender, EventArgs e) {
            RaisePropertiesChanged(nameof(ShowDropHint), nameof(PlaylistStats));
            FillPlaylist();
        }

        private void FillPlaylist(string filter = null) {
            PlaylistItems.Clear();

            Regex query;
            try {
                query = string.IsNullOrEmpty(filter) ? null : new Regex(filter, RegexOptions.IgnoreCase);
            } catch(ArgumentException) {
                query = null;
            }

            foreach(var addSong in Playlist) {
                var svm = new SongViewModel(addSong) {
                    Playing = addSong == SongPlayer.CurrentSong
                };
                if(query == null) {
                    PlaylistItems.Add(svm);
                } else {
                    if(query.IsMatch(svm.Title) || query.IsMatch(svm.SubTitle)) {
                        PlaylistItems.Add(svm);
                    }
                }
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