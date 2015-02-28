using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {
    struct Song {
        public string Title;
        public string Artist;
        public string Album;

        public string FilePath;

        public Song(string _SongName, string _BandName, string _AlbumName, string _FilePath) {
            if (string.IsNullOrWhiteSpace(_SongName) || string.IsNullOrWhiteSpace(_FilePath)) {
                throw new ArgumentException("Invallid, name & filepath cannot be null");
            }

            Title = _SongName;
            Artist = _BandName;
            Album = _AlbumName;
            FilePath = _FilePath;
        }

        public override bool Equals(object obj) {
            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            if (Album != null) {
                return Title.GetHashCode() + Album.GetHashCode();
            } else {
                return Title.GetHashCode();
            }
        }
    }
}
