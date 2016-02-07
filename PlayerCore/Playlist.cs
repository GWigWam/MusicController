using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

    [DebuggerDisplay("{Length} Songs, current #{CurrentSongIndex})")]
    public class Playlist {
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

        public Song[] CurrentList => Songs.ToArray();

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

        public event EventHandler ListChanged;

        public event EventHandler CurrentSongChanged;

        public Playlist() {
            random = new Random();

            Songs = new List<Song>();
        }

        public void AddSong(Song song, bool handleInternally = true) {
            if(!Songs.Contains(song, CompareSongByPath.Instance)) {
                Songs.Add(song);

                if(handleInternally) {
                    RaiseListChanged();
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
                RaiseListChanged();
            }
        }

        public void Clear() {
            Songs.Clear();
            currentSongIndex = -1;
            RaiseListChanged();
            RaiseCurrentSongChanged();
        }

        public void Shuffle() {
            if(Songs.Count > 0) {
                Songs = Songs.OrderBy(s => random.NextDouble()).ToList();
                RaiseListChanged();
            }
        }

        public void Order<TKey>(Func<Song, TKey> by, bool reverse = false) {
            if(Length > 0) {
                if(reverse) {
                    Songs = Songs.OrderBy(by).Reverse().ToList();
                } else {
                    Songs = Songs.OrderBy(by).ToList();
                }

                RaiseListChanged();
            }
        }

        public void Reverse() {
            Songs.Reverse();
            RaiseListChanged();
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

        public void Remove(IEnumerable<Song> songs) {
            var currentSongBeforeRemove = CurrentSong;
            foreach(var song in songs) {
                Remove(song, false);
            }

            HandleIndexOnRemove(currentSongBeforeRemove);
            RaiseListChanged();
        }

        public void Remove(Song song, bool handleInternally = true) {
            var currentSongBeforeRemove = CurrentSong;
            Songs.Remove(song);

            if(handleInternally) {
                HandleIndexOnRemove(currentSongBeforeRemove);
                RaiseListChanged();
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

        public void RaiseListChanged() {
            ListChanged?.Invoke(this, new EventArgs());
        }

        public void RaiseCurrentSongChanged() {
            CurrentSongChanged?.Invoke(this, new EventArgs());
        }
    }
}