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

        public void AddSong(Song song, bool handleInternally = true) {
            if(!Songs.Contains(song, CompareSongByPath.Instance)) {
                Songs.Add(song);

                if(handleInternally) {
                    RaiseListContentChanged();
                }

                if(CurrentSongIndex < 0) {
                    CurrentSongIndex = 0;
                }
            }
        }

        public void AddSongs(IEnumerable<Song> songs) {
            if(songs.Count() > 0) {
                foreach(var song in songs) {
                    AddSong(song, false);
                }
                RaiseListContentChanged();
            }
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

        public void PlayFirstMatch(Song song) {
            PlayFirstMatch((comp) => CompareSongByPath.Instance.Equals(comp, song));
        }

        public void PlayFirstMatch(Predicate<Song> filter) {
            var index = Songs.FindIndex(filter);
            if(index >= 0) {
                CurrentSongIndex = index;
            }
        }

        public void PlayAllMatches(Predicate<Song> filter) {
            if(Songs.Any(s => filter(s))) {
                var before = Songs.Take(CurrentSongIndex + 1).Where(s => !filter(s));
                var after = Songs.Skip(CurrentSongIndex + 1).Where(s => !filter(s));
                var match = Songs.Where(s => filter(s));
                Songs = new List<Song>(before.Concat(match).Concat(after));
                CurrentSongIndex = before.Count();
                RaiseListOrderChanged();
            }
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
            var currentSongBeforeRemove = CurrentSong;
            Songs.Remove(song);

            if(handleInternally) {
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