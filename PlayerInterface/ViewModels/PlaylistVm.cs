using PlayerCore;
using PlayerCore.PlaylistFiles;
using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {
    public class PlaylistVm : NotifyPropertyChanged {

        public event EventHandler DisplayedSongsChanged;

        public IBaseCommand PlaySongCommand { get; }
        public IBaseCommand SortByCommand { get; }
        public IBaseCommand ReverseSortCommand { get; }
        public IBaseCommand ShuffleCommand { get; }
        public IBaseCommand SortBySearchCommand { get; }        
        public IBaseCommand AddFilesCommand { get; }
        public IBaseCommand RemoveSongsCommand { get; }
        public IBaseCommand ExportCommand { get; }
        public IBaseCommand PlayTopResultCommand { get; }
        public IBaseCommand QueueTopResultCommand { get; }

        public RelayCommand<(SongViewModel[] move, SongViewModel to)> MovePlaylistSongsCommand { get; }

        private string _searchText = string.Empty;
        public string SearchText {
            get => _searchText;
            set {
                if (value != _searchText) {
                    _searchText = value;
                    HandleSearchChanged();
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SongViewModel> AllPlaylistItems { get; }

        public ObservableCollection<SongViewModel> PlaylistItems { get; private set; }

        public IEnumerable<SongViewModel> SelectedPlaylistItems => AllPlaylistItems.Where(svm => svm.IsSelected);

        private readonly Playlist _playlist;
        private readonly AppSettings _settings;

        private Action<Song> _playSong;

        public PlaylistVm(AppSettings settings, Playlist playlist, SongPlayer songPlayer, Action<bool> setUiEnabled, Action<Song> playSong) {
            _playlist = playlist;
            _settings = settings;
            _playSong = playSong;

            _playlist.ListContentChanged += PlaylistChanged;
            _playlist.ListOrderChanged += PlaylistChanged;
            _playlist.QueueChanged += (_, __) => UpdateQueueDisplay();

            AllPlaylistItems = new ObservableCollection<SongViewModel>();
            PlaylistItems = AllPlaylistItems;

            SortByCommand = new RelayCommand<PropertyInfo>(SortByProperty);

            ReverseSortCommand = new RelayCommand(() => _playlist.Reverse());

            MovePlaylistSongsCommand = new RelayCommand<(SongViewModel[] move, SongViewModel to)>(
                inp => _playlist.MoveTo(inp.to.Song, inp.move.Select(svm => svm.Song).ToArray()),
                inp => (inp.move?.Length ?? 0) > 0 && inp.to?.Song != null
            );

            ShuffleCommand = new RelayCommand(Shuffle);

            var sbsc = new RelayCommand(
                SortBySearch,
                () => !string.IsNullOrEmpty(SearchText)
            );
            sbsc.BindCanExecuteToProperty(h => PropertyChanged += h, nameof(SearchText));
            SortBySearchCommand = sbsc;

            AddFilesCommand = new AsyncCommand<dynamic>(async (dyn) => {
                dynamic input = dyn;
                setUiEnabled(false);
                var paths = input.Paths as string[];
                Song position = (input.Position as Song) ?? _playlist.LastOrDefault();
                if (paths != null) {
                    var addFiles = await Task.Run(() => SongPathsHelper.CreateSongs(_settings, paths).ToArray());
                    var added = _playlist.AddSong(addFiles
                        .OrderBy(s => s.Artist)
                        .ThenBy(s => s.File.Year)
                        .ThenBy(s => s.Album)
                        .ThenBy(s => s.File.Disc)
                        .ThenBy(s => s.File.Track));
                    _playlist.MoveTo(position, added.ToArray());
                }
            }, (t) => {
                setUiEnabled(true);
                if (t.IsFaulted) {
                    Application.Current.Dispatcher.Invoke(() => new ExceptionWindow(t.Exception).Show());
                }
            });

            RemoveSongsCommand = new RelayCommand<IEnumerable<SongViewModel>>(
                execute: o => _playlist.Remove(o.Select(svm => svm.Song)),
                canExecute: o => o?.Count() > 0
            );

            ExportCommand = new AsyncCommand(Export);

            PlayTopResultCommand = new RelayCommand(() => {
                if(PlaylistItems.FirstOrDefault() is SongViewModel fst) {
                    playSong(fst.Song);
                }
            });

            QueueTopResultCommand = new RelayCommand(() => {
                if(PlaylistItems.FirstOrDefault() is SongViewModel fst) {
                    Enqueue(fst);
                }
            });

            songPlayer.SongChanged += (_, a) => UpdateCurrentSong(a.Next);

            FillPlaylist();
            UpdateCurrentSong(songPlayer.CurrentSong);
            UpdateQueueDisplay();
        }

        private void Shuffle() {
            var selected = SelectedPlaylistItems.Select(svm => svm.Song).ToArray();
            selected = selected.Length > 1 ? selected : null;
            _playlist.Shuffle(selected);
        }

        private void SortByProperty(PropertyInfo pi) {
            var selected = SelectedPlaylistItems.Select(svm => svm.Song).ToArray();
            selected = selected.Length > 1 ? selected : null;

            if (pi.DeclaringType == typeof(Song)) {
                _playlist.Order((s) => pi.GetValue(s), selected);
            } else if (pi.DeclaringType == typeof(SongFile)) {
                _playlist.Order((s) => pi.GetValue(s.File), selected);
            } else if (pi.DeclaringType == typeof(SongStats)) {
                _playlist.Order((s) => pi.GetValue(s.Stats), selected);
            }
        }

        private void SortBySearch() {
            var selected = SelectedPlaylistItems.Select(svm => svm.Song).ToArray();
            selected = selected.Length > 1 ? selected : null;

            var reg = new Regex(SearchText, RegexOptions.IgnoreCase);
            SearchText = string.Empty;
            _playlist.Order(s => {
                var res = 0;
                res += (reg.IsMatch(s.Title ?? string.Empty) ? 0 : 3);
                res += (reg.IsMatch(s.Artist ?? string.Empty) ? 0 : 2);
                res += (reg.IsMatch(s.Album ?? string.Empty) ? 0 : 1);
                return res;
            }, selected);
        }

        private void HandleSearchChanged() {
            if (string.IsNullOrEmpty(SearchText)) {
                if (PlaylistItems != AllPlaylistItems) {
                    PlaylistItems = AllPlaylistItems;
                    RaisePropertyChanged(nameof(PlaylistItems));
                }
            } else {
                PlaylistItems = new ObservableCollection<SongViewModel>(GetSearchResult());
                RaisePropertyChanged(nameof(PlaylistItems));
            }
        }

        private void PlaylistChanged(object sender, EventArgs e) {
            SearchText = string.Empty;
            FillPlaylist();
        }

        private void FillPlaylist() {
            foreach (var removed in AllPlaylistItems.Where(svm => !_playlist.Any(s => s.FilePath == svm.Path)).ToArray()) {
                AllPlaylistItems.Remove(removed);
            }

            IEnumerable<(Song song, SongViewModel svm)> Match() {
                for (int i = 0; i < _playlist.Length; i++) {
                    var s = _playlist[i];
                    var vm = AllPlaylistItems.FirstOrDefault(svm => svm.Path == s.FilePath);
                    yield return (s, vm);
                }
            }
            var matched = Match().ToArray();

            for (int i = 0; i < matched.Length; i++) {
                var curMatch = matched[i];
                var curSvmIndex = AllPlaylistItems.IndexOf(curMatch.svm);
                if (curSvmIndex >= 0) {
                    AllPlaylistItems.Move(curSvmIndex, i);
                } else {
                    bool IsCurrentSong(Song s) => _playlist.CurrentSong == s;
                    void RemoveSong(Song s) => _playlist.Remove(s);

                    var newSvm = new SongViewModel(curMatch.song, _settings, IsCurrentSong, _playSong, Enqueue, RemoveSong);
                    AllPlaylistItems.Insert(i, newSvm);
                }
            }

            DisplayedSongsChanged?.Invoke(this, new EventArgs());
        }

        public void Enqueue(Song s) {
            _playlist.Enqueue(s);
        }

        private IEnumerable<SongViewModel> GetSearchResult() {
            Regex query = null;
            try {
                query = string.IsNullOrEmpty(SearchText) ? null : new Regex(SearchText, RegexOptions.IgnoreCase);
            } catch (ArgumentException) { }

            if (query != null) {
                return AllPlaylistItems.Where(pli => query.IsMatch(pli.Title) || query.IsMatch(pli.SubTitle));
            } else {
                return AllPlaylistItems;
            }
        }

        private void UpdateCurrentSong(Song currentSong) {
            var curPlaying = AllPlaylistItems.FirstOrDefault(svm => svm.Playing);
            if (curPlaying != null)
                curPlaying.Playing = false;

            var newPlaying = AllPlaylistItems.FirstOrDefault(svm => svm.Song == currentSong);
            if (newPlaying != null) {
                newPlaying.Playing = true;
            }
        }

        private void UpdateQueueDisplay() {
            foreach (var svm in AllPlaylistItems) {
                var i = _playlist.Queue.IndexOf(svm.Song);
                svm.QueueIndex = i >= 0 ? (i + 1) : (int?)null;
            }
        }

        private async Task Export() {
            var sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.FileName = "MyPlaylist";
            sfd.DefaultExt = ".m3u";
            sfd.Filter = "Playlist (.m3u)|*.m3u";

            if (sfd.ShowDialog() == true) {
                var m3u = new M3U(_playlist.Select(s => s.File));
                await m3u.WriteAsync(sfd.FileName, true);
            }
        }
    }
}
