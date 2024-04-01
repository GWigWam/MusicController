using System;
using System.Diagnostics;

#nullable enable
namespace PlayerCore.Songs
{
    [DebuggerDisplay("{Album} - {Artist} [{BitRate}kbps]")]
    public record SongTags
    {
        public TimeSpan TrackLength { get; init; }
        public int BitRate { get; init; }

        public string? Title { get; init; }
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
    }
}
