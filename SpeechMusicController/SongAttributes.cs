using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {

    [DebuggerDisplay("{Title} - {Artist} ({Album})")]
    public struct SongAttributes {
        public string Title;
        public string Artist;
        public string Album;

        public SongAttributes(string title, string artist, string album) {
            Title = title ?? string.Empty;
            Artist = artist ?? string.Empty;
            Album = album ?? string.Empty;
        }

        public override int GetHashCode() {
            return Title.GetHashCode() + Artist.GetHashCode() + Album.GetHashCode();
        }

        public override bool Equals(object obj) {
            return GetHashCode() == obj.GetHashCode();
        }

        public static bool operator ==(SongAttributes s1, SongAttributes s2) {
            return s1.Equals(s2);
        }

        public static bool operator !=(SongAttributes s1, SongAttributes s2) {
            return !(s1 == s2);
        }
    }
}