using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Songs {

    public class Song {
        public SongFile File { get; }

        private string title;

        public string Title {
            get {
                return title ?? File.Title;
            }
            set {
                title = value;
            }
        }

        private string artist;

        public string Artist {
            get {
                return artist ?? File.Artist;
            }
            set {
                artist = value;
            }
        }

        private string album;

        public string Album {
            get {
                return album ?? File.Album;
            }
            set {
                album = value;
            }
        }

        public string FilePath => File.Path;

        public Song(SongFile file) {
            File = file;
        }
    }
}