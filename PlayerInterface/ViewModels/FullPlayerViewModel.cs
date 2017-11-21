using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Commands;
using SpeechControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class FullPlayerViewModel : SmallPlayerViewModel {

        public event EventHandler DisplayedSongsChanged;

        public string TrackLengthStr => FormatHelper.FormatTimeSpan(SongPlayer.TrackLength);

        public string StatusText => $"{SongPlayer?.CurrentSong?.Title} - {SongPlayer?.CurrentSong?.Artist}";

        public Visibility ShowDropHint => (Playlist?.Length ?? 0) > 0 ? Visibility.Collapsed : Visibility.Visible;

        private string _searchText;

        public string SearchText {
            get { return _searchText; }
            set {
                if(value != _searchText) {
                    _searchText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand PlaySongCommand {
            get; private set;
        }

        public ICommand ReverseSortCommand {
            get; private set;
        }

        public ICommand SortByCommand {
            get; private set;
        }

        public ICommand SortBySearchCommand {
            get; private set;
        }

        public ICommand RemoveSongsCommand {
            get; private set;
        }

        public ICommand SearchCommand {
            get; private set;
        }

        public ICommand AddFilesCommand {
            get; private set;
        }

        public ICommand ShuffleCommand {
            get; private set;
        }

        public GenericRelayCommand<Tuple<SongViewModel[], SongViewModel>> MovePlaylistSongsCommand {
            get; private set;
        }

        public SongViewModel CurrentFocusItem {
            get; set;
        }

        /// <summary>
        /// Items for the visible playlist WARNING apparantly ObservableCollection isn't thread safe :(
        /// </summary>
        public ObservableCollection<SongViewModel> PlaylistItems {
            get; private set;
        }

        public ObservableCollection<string> AboutSpeechCommands {
            get; private set;
        }

        public string PlaylistStats => $"{Playlist?.Length} - {FormatHelper.FormatTimeSpan(new TimeSpan(Playlist?.Sum(s => s.File.TrackLength.Ticks) ?? 0))}";

        public AppSettingsViewModel SettingsViewModel {
            get;
        }

        private bool _UIEnabled;

        public bool UIEnabled {
            get { return _UIEnabled; }
            set {
                if(_UIEnabled != value) {
                    _UIEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FullPlayerViewModel(AppSettings settings, SongPlayer player, Playlist playlist, SpeechController speechController, TransitionManager transitionMngr) : base(settings, player, playlist, transitionMngr) {
            SetupCommands();

            SettingsViewModel = new AppSettingsViewModel(Settings);

            FillPlaylist();

            SetupAboutSpeechCommands(speechController);

            SongPlayer.SongChanged += SongPlayer_SongChanged;
            Playlist.ListContentChanged += PlaylistChanged;
            Playlist.ListOrderChanged += PlaylistChanged;

            SearchText = string.Empty;
            UIEnabled = true;
        }

        private void SetupCommands() {
            PlaySongCommand = new AsyncCommand(
                execute: (s) => {
                    UIEnabled = false;
                    if(s as Song != null) {
                        Playlist.SelectFirstMatch((Song)s);
                        if(SongPlayer.PlayerState != PlayerState.Playing) {
                            SongPlayer.PlayerState = PlayerState.Playing;
                        }
                    }
                },
                continueWith: t => UIEnabled = true,
                canExecute: (s) => SongPlayer != null && s as Song != null
            );

            ReverseSortCommand = new RelayCommand((o) => Playlist.Reverse(), (o) => Playlist != null);

            SortByCommand = new RelayCommand(
                (o) => {
                    var pi = o as PropertyInfo;
                    if(pi.DeclaringType == typeof(Song)) {
                        Playlist.Order((s) => pi.GetValue(s));
                    } else if(pi.DeclaringType == typeof(SongFile)) {
                        Playlist.Order((s) => pi.GetValue(s.File));
                    } else if(pi.DeclaringType == typeof(SongStats)) {
                        Playlist.Order((s) => pi.GetValue(s.Stats));
                    }
                },
                (o) => o as PropertyInfo != null && (
                    ((PropertyInfo)o).DeclaringType == typeof(Song) ||
                    ((PropertyInfo)o).DeclaringType == typeof(SongFile) ||
                    ((PropertyInfo)o).DeclaringType == typeof(SongStats))
            );

            SortBySearchCommand = new RelayCommand(
                _ => {
                    var reg = new Regex(SearchText, RegexOptions.IgnoreCase);
                    SearchText = string.Empty;
                    Playlist.Order(s => {
                        var res = 0;
                        res += (reg.IsMatch(s.Title ?? string.Empty) ? 0 : 3);
                        res += (reg.IsMatch(s.Artist ?? string.Empty) ? 0 : 2);
                        res += (reg.IsMatch(s.Album ?? string.Empty) ? 0 : 1);
                        return res;
                    });
                },
                _ => !string.IsNullOrEmpty(SearchText)
            );

            RemoveSongsCommand = new RelayCommand((o) => {
                Playlist.Remove(((IEnumerable<SongViewModel>)o).Select(svm => svm.Song));
            }, (o) => {
                var songs = o as IEnumerable<SongViewModel>;
                return songs != null && songs.Count() > 0;
            });

            SearchCommand = new RelayCommand((o) => FillPlaylist(o as string));

            AddFilesCommand = new AsyncCommand(async (dyn) => {
                dynamic input = dyn;
                UIEnabled = false;
                var paths = input.Paths as string[];
                Song position = (input.Position as Song) ?? Playlist.LastOrDefault();
                if(paths != null) {
                    var addFiles = await SongFileReader.ReadFilePathsAsync(Settings, paths);
                    var added = Playlist.AddSong(addFiles);
                    Playlist.MoveTo(position, added.ToArray());
                }
            }, (t) => {
                UIEnabled = true;
                if(t.IsFaulted)
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => new ExceptionWindow(t.Exception).Show()));
            });

            MovePlaylistSongsCommand = new GenericRelayCommand<Tuple<SongViewModel[], SongViewModel>>(inp => {
                Playlist.MoveTo(inp.Item2.Song, inp.Item1.Select(svm => svm.Song).ToArray());
            }, (inp) => {
                return (inp.Item1?.Length ?? 0) > 0 && inp.Item2?.Song != null;
            });

            ShuffleCommand = new RelayCommand((o) => Playlist.Shuffle(), (o) => Playlist != null);
        }

        private void SetupAboutSpeechCommands(SpeechController speechController) {
            AboutSpeechCommands = new ObservableCollection<string>();
            LoadAboutSpeechCommands(speechController);
            speechController.Commands.CollectionChanged += (s, a) => LoadAboutSpeechCommands(speechController);
        }

        private void LoadAboutSpeechCommands(SpeechController speechController) {
            var commandDesc = speechController.Commands.Select(sc => sc.Description);
            AboutSpeechCommands.Clear();
            foreach(var desc in commandDesc) {
                AboutSpeechCommands.Add(desc);
            }
        }

        private void PlaylistChanged(object sender, EventArgs e) {
            RaisePropertiesChanged(nameof(ShowDropHint), nameof(PlaylistStats));
            Application.Current.Dispatcher.BeginInvoke((Action)(() => FillPlaylist()));
        }

        private void FillPlaylist(string filter = null) {
            if(PlaylistItems == null) {
                PlaylistItems = new ObservableCollection<SongViewModel>();
            }
            PlaylistItems.Clear();

            Regex query;
            try {
                query = string.IsNullOrEmpty(filter) ? null : new Regex(filter, RegexOptions.IgnoreCase);
            } catch(ArgumentException) {
                query = null;
            }

            foreach(var addSong in Playlist) {
                var svm = new SongViewModel(addSong, this) {
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

            DisplayedSongsChanged?.Invoke(this, new EventArgs());
        }

        private void SongPlayer_SongChanged(object sender, EventArgs e) {
            var curPlaying = PlaylistItems.FirstOrDefault(svm => svm.Playing);
            if(curPlaying != null)
                curPlaying.Playing = false;

            var newPlaying = PlaylistItems.FirstOrDefault(svm => svm.Song == SongPlayer.CurrentSong);
            if(newPlaying != null) {
                newPlaying.Playing = true;
                CurrentFocusItem = newPlaying;
                RaisePropertyChanged(nameof(CurrentFocusItem));
            }

            RaisePropertiesChanged(nameof(TrackLengthStr), nameof(StatusText), nameof(EnableChangeElapsed));
        }
    }
}