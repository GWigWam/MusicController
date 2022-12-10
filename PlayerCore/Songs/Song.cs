using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.Songs
{
    [DebuggerDisplay("{Title} - {Artist} ({Album})")]
    public record Song : IEqualityComparer<Song>, IEquatable<Song>
    {
        public string Path { get; }
        public string Title { get; }
        public TimeSpan TrackLength { get; }
        public int BitRate { get; init; }

        public string? Album { get; init; }
        public string? Artist { get; init; }
        public string? Genre { get; init; }

        public int? Track { get; init; }
        public int? TrackCount { get; init; }
        public int? Disc { get; init; }
        public int? DiscCount { get; init; }
        public int? Year { get; init; }
        public double? AlbumGain { get; init; }
        public double? TrackGain { get; init; }

        internal Song(string path, string title, TimeSpan trackLength)
        {
            Path = path;
            Title = title;
            TrackLength = trackLength;
        }

        public static Task<Song?> CreateAsync(string filePath) => SongFileFactory.GetAsync(filePath);

        public virtual bool Equals(Song? other) => Equals(this, other);
        public bool Equals(Song? x, Song? y) => x?.GetHashCode() == y?.GetHashCode();
        public int GetHashCode(Song obj) => obj.GetHashCode();
        public override int GetHashCode() => Path.ToLower().GetHashCode();
    }
}
