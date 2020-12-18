using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Songs {

    [DebuggerDisplay("{Title} - {Artist} ({Album})")]
    public class Song {

        public SongFile File { get; }
        public SongStats Stats { get; }

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

        public Song(SongFile file, AppSettings settings) {
            File = file ?? throw new ArgumentNullException(nameof(file));

            Stats = settings.SongStats.FirstOrDefault(ss => ss.Path.Equals(FilePath, StringComparison.InvariantCultureIgnoreCase));
            if(Stats == null) {
                Stats = new SongStats(FilePath);
                settings.AddSongStats(Stats);
            }
        }
    }

    public class CompareSongByPath : IEqualityComparer<Song> {
        public static CompareSongByPath Instance = new CompareSongByPath();

        public bool Equals(Song x, Song y) {
            return x.FilePath.Equals(y.FilePath, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(Song obj) {
            return obj.FilePath.GetHashCode();
        }
    }
}
