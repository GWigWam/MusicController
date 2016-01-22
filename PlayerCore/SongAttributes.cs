using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

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

        public override int GetHashCode() => (Title ?? string.Empty).GetHashCode() + (Artist ?? string.Empty).GetHashCode() + (Album ?? string.Empty).GetHashCode();

        public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode();

        public static bool operator ==(SongAttributes s1, SongAttributes s2) => s1.Equals(s2);

        public static bool operator !=(SongAttributes s1, SongAttributes s2) => !(s1 == s2);
    }
}