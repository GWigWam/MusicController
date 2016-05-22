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

        public Song AddSong(Song song, bool handleInternally = true) {
            if(!Songs.Contains(song, CompareSongByPath.Instance)) {
                Songs.Add(song);

                if(handleInternally) {
                    RaiseListContentChanged();
                }

                if(CurrentSongIndex < 0) {
                    CurrentSongIndex = 0;
                }
                return song;
            } else {
                return null;
            }
        }

        public IEnumerable<Song> AddSongs(IEnumerable<Song> songs) {
            var addedSongs = new List<Song>();
            if(songs.Count() > 0) {
                foreach(var song in songs) {
                    var added = AddSong(song, false);
                    if(added != null) {
                        addedSongs.Add(added);
                    }
                }
                RaiseListContentChanged();
            }
            return addedSongs;
        }

        public void Clear() {
            Songs.Clear();
            currentSongIndex = -1;
            RaiseListContentChanged();
            RaiseCurrentSongChanged();
        }

        public void Shuffle() {
            if(Songs.Count > 0) {
                Songs = Songs.OrderBy(s => random.NextDouble()).ToList();
                RaiseListOrderChanged();
            }
        }

        public void Order<TKey>(Func<Song, TKey> by, bool reverse = false) {
            if(Length > 0) {
                if(reverse) {
                    Songs = Songs.OrderBy(by).Reverse().ToList();
                } else {
                    Songs = Songs.OrderBy(by).ToList();
                }

                RaiseListOrderChanged();
            }
        }

        public void Reverse() {
            Songs.Reverse();
            RaiseListOrderChanged();
        }

        public void SelectFirstMatch(Song song) {
            SelectFirstMatch((comp) => CompareSongByPath.Instance.Equals(comp, song));
        }

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
            var curSong = CurrentSong;

            var before = Songs.Take(index + 1).Except(source);
            var after = Songs.Skip(index + 1).Except(source);
            Songs = new List<Song>(before.Concat(source).Concat(after));

            currentSongIndex = Songs.IndexOf(curSong);
            RaiseListOrderChanged();
        }

        public void Remove(IEnumerable<Song> songs) {
            var currentSongBeforeRemove = CurrentSong;
            foreach(var song in songs) {
                Remove(song, false);
            }

            HandleIndexOnRemove(currentSongBeforeRemove);
            RaiseListContentChanged();
        }

        public void Remove(Song song, bool handleInternally = true) {
            Song currentSongBeforeRemove = null;
            if(handleInternally) {
                currentSongBeforeRemove = CurrentSong;
            }

            Songs.Remove(song);

            if(currentSongBeforeRemove != null) {
                HandleIndexOnRemove(currentSongBeforeRemove);
                RaiseListContentChanged();
            }
        }

        private void HandleIndexOnRemove(Song prefCurrentSong) {
            var foundIndex = Songs.FindIndex((s) => s == prefCurrentSong);
            if(foundIndex >= 0) { //Songs stays the same
                currentSongIndex = foundIndex;
            } else if(Length > 0) { //Set song to first in list
                currentSongIndex = 0;
                RaiseCurrentSongChanged();
            } else { //No songs to play
                CurrentSongIndex = -1;
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