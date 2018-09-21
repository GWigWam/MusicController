using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayerCore.Songs {

    [DebuggerDisplay("{Title}: {Path})")]
    public class SongFile : IEqualityComparer<SongFile>, IEquatable<SongFile> {
        public string Album { get; }
        public string Artist { get; }
        public string Genre { get; }
        public string Title { get; }
        public uint Track { get; }
        public uint TrackCount { get; }
        public uint Year { get; }
        public int BitRate { get; }
        public TimeSpan TrackLength { get; }

        public string Path { get; }

        internal SongFile(string path, string title, string album, string artist, string genre, uint track, uint trackCount, uint year, int bitRate, TimeSpan trackLength) {
            Album = album;
            Artist = artist;
            Genre = genre;
            Path = path;
            Title = title;
            Track = track;
            TrackCount = trackCount;
            Year = year;
            BitRate = bitRate;
            TrackLength = trackLength;
        }

        public static bool TryCreate(string filePath, out SongFile result) => SongFileFactory.TryGet(filePath, out result);

        public override bool Equals(object obj) => obj is SongFile sf ? Equals(sf) : false;

        public bool Equals(SongFile other) => Equals(this, other);

        public bool Equals(SongFile x, SongFile y) {
            return
                (x == null && y == null) ? true :
                (x == null || y == null) ? false :
                GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(SongFile obj) => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));

        public override int GetHashCode() => Path.ToLower().GetHashCode();
    }
}
