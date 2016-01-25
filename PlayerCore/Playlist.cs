using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

    public class Playlist {
        private int currentSongIndex = -1;

        public int CurrentSongIndex {
            get { return currentSongIndex; }
            set {
                if(value >= 0 && value < Lenght) {
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
                if(CurrentSongIndex >= 0 && CurrentSongIndex < Lenght) {
                    return Songs.ElementAt(CurrentSongIndex);
                } else {
                    return null;
                }
            }
        }

        public int Lenght => Songs?.Count ?? 0;

        private Random random;
        private List<Song> Songs { get; set; }

        public event EventHandler ListChanged;

        public event EventHandler CurrentSongChanged;

        public Playlist() {
            random = new Random();

            Songs = new List<Song>();
        }

        public void AddSongs(IEnumerable<Song> songs) {
            if(songs.Count() > 0) {
                Songs.AddRange(songs);
                RaiseListChanged();

                if(CurrentSongIndex < 0) {
                    CurrentSongIndex = 0;
                }
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
            if(Lenght > 0) {
                if(reverse) {
                    Songs = Songs.OrderBy(by).Reverse().ToList();
                } else {
                    Songs = Songs.OrderBy(by).ToList();
                }

                RaiseListChanged();
            }
        }

        public void PlayFirstMatch(Predicate<Song> filter) {
            var index = Songs.FindIndex(filter);
            if(index >= 0) {
                CurrentSongIndex = index;
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