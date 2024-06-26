﻿using PlayerCore;
using PlayerCore.PlaylistFiles;
using PlayerCore.Persist;
using PlayerCore.Songs;
using PlayerInterface.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.ViewModels
{
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

        public PlaylistVm(AppSettings settings, SongFileFactory songFileFactory, Playlist playlist, SongPlayer songPlayer, Action<Song> playSong)
        {
            _playlist = playlist;
            _settings = settings;
            _playSong = playSong;

            _playlist.CollectionChanged += (s, a) => HandlePlaylistCollectionChanged(a);
            _playlist.CollectionChanged += (_, _) => SearchText = string.Empty;
            _playlist.QueueChanged += (_, _) => UpdateQueueDisplay();

            AllPlaylistItems = new ObservableCollection<SongViewModel>();
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(AllPlaylistItems, AllPlaylistItems);
            PlaylistItems = AllPlaylistItems;

            SortByCommand = new RelayCommand<PropertyInfo[]>(SortByProperty);

            ReverseSortCommand = new RelayCommand(Reverse);

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

            AddFilesCommand = new AsyncCommand<dynamic>(
                async input => {
                    Song position = (input.Position as Song) ?? _playlist.LastOrDefault();
                    if(input.Paths is string[] paths)
                    {
                        await _playlist.AddSongsAsync(SongPathsHelper.CreateSongs(songFileFactory, paths), position, orderBys: new Func<Song, object>[] { s => s.Tags?.Artist, s => s.Tags?.Year, s => s.Tags?.Album, s => s.Tags?.Disc, s => s.Tags?.Track });
                    }
                },
                t => {
                    if(t.IsFaulted)
                    {
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

            HandlePlaylistCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            UpdateCurrentSong(songPlayer.CurrentSong);
            UpdateQueueDisplay();
        }

        private void Shuffle() {
            var selected = SelectedPlaylistItems.Select(svm => svm.Song).ToArray();
            selected = selected.Length > 1 ? selected : null;
            _playlist.Shuffle(selected);
        }

        private void SortByProperty(PropertyInfo[] pis) {
            var selected = SelectedPlaylistItems.Select(svm => svm.Song).ToArray();
            selected = selected.Length > 1 ? selected : null;

            foreach (var pi in pis)
            {
                if (pi.DeclaringType == typeof(Song))
                {
                    _playlist.Order((s) => pi.GetValue(s), selected);
                }
                else if (pi.DeclaringType == typeof(SongStats))
                {
                    _playlist.Order((s) => s.Stats is SongStats stats ? pi.GetValue(stats) : null, selected);
                }
                else if (pi.DeclaringType == typeof(SongTags))
                {
                    _playlist.Order((s) => s.Tags is SongTags t ? pi.GetValue(t) : null, selected);
                }
            }
        }

        private void Reverse() {
            var selected = SelectedPlaylistItems.Select(svm => svm.Song).ToArray();
            selected = selected.Length > 1 ? selected : null;

            _playlist.Reverse(selected);
        }

        private void SortBySearch() {
            var selected = SelectedPlaylistItems.Select(svm => svm.Song).ToArray();
            selected = selected.Length > 1 ? selected : null;

            var reg = new Regex(SearchText, RegexOptions.IgnoreCase);
            SearchText = string.Empty;
            _playlist.Order(
                orderBys: new Func<Song, object>[] {
                    s => !reg.IsMatch(s.Title),
                    s => string.IsNullOrEmpty(s.Tags?.Album)  || !reg.IsMatch(s.Tags.Album),
                    s => string.IsNullOrEmpty(s.Tags?.Artist) || !reg.IsMatch(s.Tags.Artist),
                },
                source: selected
            );
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

        private void HandlePlaylistCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            SongViewModel findSvm(Song song) => AllPlaylistItems.First(i => i.Song == song);

            lock(AllPlaylistItems)
            {
                if(e.Action == NotifyCollectionChangedAction.Add)
                {
                    AllPlaylistItems.InsertRange(e.NewStartingIndex, e.NewItems.OfType<Song>().Select(CreateSvm));
                }
                else if(e.Action == NotifyCollectionChangedAction.Remove)
                {
                    AllPlaylistItems.RemoveRange(e.OldItems.OfType<Song>().Select(findSvm));
                }
                else if(e.Action == NotifyCollectionChangedAction.Move)
                {
                    var moved = e.NewItems.OfType<Song>().Select(findSvm).First();
                    AllPlaylistItems.RemoveAt(e.OldStartingIndex);
                    AllPlaylistItems.Insert(e.NewStartingIndex, moved);
                }
                else if(e.Action == NotifyCollectionChangedAction.Reset)
                {
                    AllPlaylistItems.Clear();
                    AllPlaylistItems.AddRange(_playlist.Select(CreateSvm));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            DisplayedSongsChanged?.Invoke(this, new EventArgs());
        }

        private SongViewModel CreateSvm(Song song)
        {
            var res = new SongViewModel(song, _settings, _playlist.CurrentSong == song, _playSong, Enqueue, s => _playlist.Remove(s));
            res.PropertyChanged += (s, a) => {
                if (a.PropertyName == nameof(SongViewModel.IsSelected))
                {
                    RaisePropertyChanged(nameof(SelectedPlaylistItems));
                }
            };
            return res;
        }

        public void Enqueue(Song s) {
            _playlist.Enqueue(s);
        }

        public void Enqueue(IEnumerable<Song> qs)
        {
            foreach (var q in qs)
            {
                Enqueue(q);
            }
        }

        private IEnumerable<SongViewModel> GetSearchResult() {
            Regex query = null;
            try {
                query = string.IsNullOrEmpty(SearchText) ? null : new Regex(SearchText, RegexOptions.IgnoreCase);
            } catch (ArgumentException) { }

            if (query != null) {
                return AllPlaylistItems
                    .Where(pli => query.IsMatch(pli.Title) || query.IsMatch(pli.SubTitle))
                    .OrderBy(pli => query.IsMatch(pli.Title) ? 0 : 1);
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
                var m3u = new M3U(_playlist.Select(s => s.Path));
                await m3u.WriteAsync(sfd.FileName, true);
            }
        }
    }
}
