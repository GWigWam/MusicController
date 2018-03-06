using PlayerCore.Songs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

    [DebuggerDisplay("{Length} Songs, current #{CurrentSongIndex})")]
    public class Playlist : IEnumerable<Song> {

        public event EventHandler ListOrderChanged;
        public event EventHandler ListContentChanged;
        public event EventHandler CurrentSongChanged;

        private Random random;

        private List<Song> Songs { get; set; }

        private int _curIndx = -1;
        /// <summary>
        /// CurrentSong is the song at CurrentSongIndex in the Playlist.
        /// If index is set to a DIFFERENT value the CurrentSongChanged event will be raised
        /// Set to -1 for no song
        /// </summary>
        private int CurrentIndex {
            get { return _curIndx; }
            set {
                if(value >= -1 && value < Length) {
                    if(_curIndx != value) {
                        _curIndx = value;
                        RaiseCurrentSongChanged();
                    }
                }
            }
        }

        private List<Song> Queue { get; set; }

        private int? _queueIndx;
        public int? QueueIndex {
            get => _queueIndx;
            set {
                if (_queueIndx != value) {
                    _queueIndx = value;
                    RaiseCurrentSongChanged();
                }
            }
        }

        public Song CurrentSong {
            get {
                if (QueueIndex.HasValue) {
                    return Queue[QueueIndex.Value];
                } else if(CurrentIndex >= 0 && CurrentIndex < Length) {
                    return Songs.ElementAt(CurrentIndex);
                } else {
                    return null;
                }
            }
        }

        public Song this[int index] => Songs[index];

        public int Length => Songs?.Count ?? 0;

        public Playlist() {
            random = new Random();
            Songs = new List<Song>();
            Queue = new List<Song>();
        }

        public IEnumerable<Song> AddSong(IEnumerable<Song> songs) => AddSong(songs.ToArray());

        public IEnumerable<Song> AddSong(params Song[] songs) {
            var addedSongs = new List<Song>();
            ChangeList(false, true, () => {
                foreach(var song in songs.Except(Songs, CompareSongByPath.Instance)) {
                    Songs.Add(song);
                    addedSongs.Add(song);
                }
            });
            return addedSongs;
        }

        public void Enqueue(Song song) {
            Queue.Add(song);
        }

        public void Clear() {
            Queue.Clear();
            ChangeList(true, true, Songs.Clear);
        }

        public void Shuffle() {
            Order(s => random.NextDouble());
            if(Length > 0) {
                SetIndexForceUpdate(0);
            }
        }

        public void Order<TKey>(Func<Song, TKey> orderBy) {
            ChangeList(true, false, () => {
                Songs = Songs.OrderBy(orderBy).ToList();
            });
        }

        public void Reverse() => ChangeList(true, false, Songs.Reverse);

        public void SelectFirstMatch(Song song) => SelectFirstMatch((comp) => CompareSongByPath.Instance.Equals(comp, song));

        public void SelectFirstMatch(Predicate<Song> filter) {
            var index = Songs.FindIndex(filter);
            if(index >= 0) {
                SetIndexForceUpdate(index);
            }
        }

        public void SelectAllMatches(Predicate<Song> filter) {
            if(Songs.Any(s => filter(s))) {
                var before = Songs.Take(CurrentIndex + 1).Where(s => !filter(s));
                var after = Songs.Skip(CurrentIndex + 1).Where(s => !filter(s));
                var match = Songs.Where(s => filter(s));
                Songs = new List<Song>(before.Concat(match).Concat(after));
                SetIndexForceUpdate(before.Count());
                RaiseListOrderChanged();
            }
        }

        public void MoveTo(Song destination, params Song[] source) {
            var index = destination == null ? 0 : Songs.IndexOf(destination);
            MoveToIndex(index, source);
        }

        public void MoveToIndex(int index, params Song[] source) {
            ChangeList(true, false, () => {
                var before = Songs.Take(index + 1).Except(source);
                var after = Songs.Skip(index + 1).Except(source);
                Songs = new List<Song>(before.Concat(source).Concat(after));
            });
        }

        public void Remove(IEnumerable<Song> songs) {
            ChangeList(false, true, () => {
                foreach(var song in songs) {
                    Remove(song, false);
                }
            });
        }

        public void Remove(Song song, bool handleInternally = true) {
            Queue.Remove(song);
            if(handleInternally) {
                ChangeList(false, true, () => {
                    Songs.Remove(song);
                });
            } else {
                Songs.Remove(song);
            }
        }

        public bool HasNext(bool loop) => GetDoNext(loop) != null;
        public bool Next(bool loop) {
            var doNext = GetDoNext(loop);
            if (doNext != null) {
                doNext();
                return true;
            } else {
                return false;
            }
        }

        public bool HasPrevious(bool loop) => GetDoPrevious(loop) != null;
        public bool Previous(bool loop) {
            var doPrev = GetDoPrevious(loop);
            if (doPrev != null) {
                doPrev();
                return true;
            } else {
                return false;
            }
        }

        private Action GetDoNext(bool loop) {
            Action GetUpdateCurrentIndexAction() {
                if (Songs.Count > 1) {
                    if (CurrentIndex + 1 < Songs.Count) {
                        return () => CurrentIndex++;
                    } else if (loop && CurrentIndex + 1 == Songs.Count) {
                        return () => CurrentIndex = 0;
                    }
                }
                return null;
            }

            if (QueueIndex.HasValue) {
                if (QueueIndex + 1 < Queue.Count) {
                    return () => QueueIndex++;
                } else {
                    return () => {
                        QueueIndex = null;
                        Queue.Clear();
                    };
                }
            } else {
                if (Queue.Count == 0) {
                    return GetUpdateCurrentIndexAction();
                } else {
                    return () => {
                        GetUpdateCurrentIndexAction()?.Invoke();
                        QueueIndex = 0;
                    };
                }
            }
        }

        private Action GetDoPrevious(bool loop) {
            if (QueueIndex.HasValue) {
                if (QueueIndex > 0) {
                    return () => QueueIndex--;
                } else {
                    return () => QueueIndex = null;
                }
            } else {
                if (Songs.Count > 1) {
                    if (CurrentIndex > 0) {
                        return () => CurrentIndex--;
                    } else if (loop && CurrentIndex == 0) {
                        return () => CurrentIndex = Songs.Count - 1;
                    }
                }
            }
            return null;
        }

        private void ChangeList(bool orderChanging, bool contentChanging, Action change) {
            var orgIndex = CurrentIndex;
            var orgLenght = Length;
            var orgSong = CurrentSong;
            change();

            var foundIndex = orgSong != null ? Songs.FindIndex((s) => s == orgSong) : -1;
            if(foundIndex > -1) {
                _curIndx = foundIndex;
            } else {
                var newIndex = (orgIndex - (orgLenght - Length)) + 1;
                newIndex = newIndex < Length ? newIndex : (Length > 0 ? Length - 1 : -1);
                SetIndexForceUpdate(newIndex);
            }

            if(orderChanging) {
                RaiseListOrderChanged();
            }
            if(contentChanging) {
                RaiseListContentChanged();
            }
        }

        /// <summary>
        /// Changes CurrentSongIndex, always raises CurrentSongChanged event, even if index stays the same.
        /// This is usefull when the list has changed since the last time the index was set and a new song is at the current index.
        /// </summary>
        /// <param name="newIndex">Nr to set CurrentSongIndex to, -1 for no song.</param>
        private void SetIndexForceUpdate(int newIndex) {
            _curIndx = (_curIndx == -1) ? -2 : -1;
            CurrentIndex = newIndex;

            QueueIndex = null;
            Queue.Clear();
        }

        public void RaiseListContentChanged() {
            ListContentChanged?.Invoke(this, new EventArgs());
        }

        public void RaiseListOrderChanged() {
            ListOrderChanged?.Invoke(this, new EventArgs());
        }

        public void RaiseCurrentSongChanged() {
            CurrentSongChanged?.Invoke(this, new EventArgs());
        }

        public IEnumerator<Song> GetEnumerator() {
            return Songs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}