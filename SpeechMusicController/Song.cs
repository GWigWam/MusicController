using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {

    public struct Song {
        public string FilePath;

        public SongAttributes Attributes;

        public Song(string title, string artist, string album, string filePath) {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(filePath)) {
                throw new ArgumentException("Invallid, name & filepath cannot be null");
            }
            Attributes = new SongAttributes(title, artist, album);
            FilePath = filePath;
        }

        public override string ToString() {
            return string.Format("{0} - {1} ({2})", Attributes.Title, Attributes.Artist, Attributes.Album);
        }
    }
}