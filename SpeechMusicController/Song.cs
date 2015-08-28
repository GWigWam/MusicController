using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {

    public class Song {
        public Uri FilePath;

        public SongAttributes Attributes { get; private set; }

        public string Title {
            get {
                return Attributes.Title;
            }
            set {
                Attributes = new SongAttributes(value, Attributes.Artist, Attributes.Album);
            }
        }

        public string Artist {
            get {
                return Attributes.Artist;
            }
            set {
                Attributes = new SongAttributes(Attributes.Title, value, Attributes.Album);
            }
        }

        public string Album {
            get {
                return Attributes.Album;
            }
            set {
                Attributes = new SongAttributes(Attributes.Title, Attributes.Artist, value);
            }
        }

        public Song(string title, string artist, string album, string filePath)
            : this(title, artist, album) {
            if(Uri.IsWellFormedUriString(filePath, UriKind.Absolute)) {
                throw new ArgumentException("Invallid filepath");
            }
            FilePath = new Uri(filePath, UriKind.Absolute);
        }

        public Song(string title, string artist, string album, Uri filePath)
            : this(title, artist, album) {
            FilePath = filePath;
        }

        //Use for creating everything except FilePath
        private Song(string title, string artist, string album) {
            if(string.IsNullOrWhiteSpace(title)) {
                throw new ArgumentException("Invallid, name cannot be null");
            }
            Attributes = new SongAttributes(title, artist, album);
        }

        public override string ToString() => $"{Attributes.Title} - {Attributes.Artist} ({Attributes.Album})";
    }

    public class SongComparerByTitleAndArtist : IEqualityComparer<Song> {
        private const int CharMultiplier = 90;

        // In most cases (false) only 1 calculation will be nessisary
        public bool Equals(Song x, Song y) => GetHashCode(x) == GetHashCode(y);

        public int GetHashCode(Song obj) {
            if(object.ReferenceEquals(obj, null)) {
                return 0;
            } else {
                return obj.Attributes.Title.GetHashCode() * obj.Attributes.Artist.GetHashCode();
            }
        }
    }
}