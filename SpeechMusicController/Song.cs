using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {

    internal struct Song {
        private readonly int HashCode;
        public string Title;

        public string Artist;
        public string Album;

        public string FilePath;

        public Song(string _Title, string _Artist, string _Album, string _FilePath) {
            if (string.IsNullOrWhiteSpace(_Title) || string.IsNullOrWhiteSpace(_FilePath)) {
                throw new ArgumentException("Invallid, name & filepath cannot be null");
            }

            Title = _Title;
            Artist = _Artist;
            Album = _Album;
            FilePath = _FilePath;

            //Hashcode
            if (Album != null) {
                HashCode = Title.GetHashCode() + Album.GetHashCode();
            } else {
                HashCode = Title.GetHashCode();
            }
        }

        public override string ToString() {
            return string.Format("{0} - {1} ({2})", Title, Artist, Album);
        }

        public override bool Equals(object obj) {
            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            return HashCode;
        }
    }
}