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
        private int currentSongIndex = -1;

        /// <summary>
        /// CurrentSong is the song at CurrentSongIndex in the Playlist.
        /// If index is set to a DIFFERENT value the CurrentSongChanged event will be raised
        /// Set to -1 for no song
        /// </summary>
        public int CurrentSongIndex {
            get { return currentSongIndex; }
            set {
                if(value >= -1 && value < Length) {
                    if(currentSongIndex != value) {
                        currentSongIndex = value;
                        RaiseCurrentSongChanged();
                    }
                }
            }
        }

        public Song CurrentSong {
            get {
                if(CurrentSongIndex >= 0 && CurrentSongIndex < Length) {
                    return Songs.ElementAt(CurrentSongIndex);
                } else {
                    return null;
                }
            }
        }

        public int Length => Songs?.Count ?? 0;

        public bool HasNext => CurrentSongIndex < (Length - 1);
        public bool HasPrevious => CurrentSongIndex > 0;

        private Random random;
        private List<Song> Songs { get; set; }

        public event EventHandler ListOrderChanged;

        public event EventHandler ListContentChanged;

        public event EventHandler CurrentSongChanged;

        public Playlist() {
            random = new Random();
            Songs = new List<Song>();
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

        public void Clear() => ChangeList(true, true, Songs.Clear);

        public void Shuffle() => Order(s => random.NextDouble());

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
                var before = Songs.Take(CurrentSongIndex + 1).Where(s => !filter(s));
                var after = Songs.Skip(CurrentSongIndex + 1).Where(s => !filter(s));
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
            if(handleInternally) {
                ChangeList(false, true, () => {
                    Songs.Remove(song);
                });
            } else {
                Songs.Remove(song);
            }
        }

        private void ChangeList(bool orderChanging, bool contentChanging, Action change) {
            var orgIndex = CurrentSongIndex;
            var orgLenght = Length;
            var orgSong = CurrentSong;
            change();

            var foundIndex = orgSong != null ? Songs.FindIndex((s) => s == orgSong) : -1;
            if(foundIndex > -1) {
                currentSongIndex = foundIndex;
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
            currentSongIndex = (currentSongIndex == -1) ? -2 : -1;
            CurrentSongIndex = newIndex;
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