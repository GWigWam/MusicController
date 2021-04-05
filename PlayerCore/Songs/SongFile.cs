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
        public int Track { get; }
        public int TrackCount { get; }
        public int Disc { get; }
        public int DiscCount { get; }
        public int Year { get; }
        public int BitRate { get; }
        public double? AlbumGain { get; }
        public double? TrackGain { get; }
        public TimeSpan TrackLength { get; }

        public string Path { get; }

        internal SongFile(string path, string title, string album, string artist, string genre, int track, int trackCount, int disc, int discCount, int year, int bitRate, double? albumGain, double? trackGain, TimeSpan trackLength)
        {
            Album = album;
            Artist = artist;
            Genre = genre;
            Path = path;
            Title = title;
            Track = track;
            TrackCount = trackCount;
            Disc = disc;
            DiscCount = discCount;
            Year = year;
            BitRate = bitRate;
            AlbumGain = albumGain;
            TrackGain = trackGain;
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
