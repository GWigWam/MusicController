using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.Songs
{
    internal static class SongFileFactory
    {
        private static readonly ConcurrentDictionary<string, Song> Cache = new();

        public static async Task<Song?> GetAsync(string filePath)
        {
            filePath = filePath.ToLower();
            if(Cache.TryGetValue(filePath, out var cached))
            {
                return cached;
            }
            else if(await CreateAsync(filePath) is Song res)
            {
                Cache.TryAdd(filePath, res);
                return res;
            }
            return null;
        }

        private static Task<Song?> CreateAsync(string filePath) => Task.Run(() => Create(filePath));

        private static Song? Create(string filePath)
        {
            var file = new FileInfo(filePath);
            if(file.Exists && SongPlayer.SupportedExtensions.Any(s => s.Equals(file.Extension, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var fileInfo = TagLib.File.Create(file.FullName);
                    var result = new Song(filePath, fileInfo.Tag.Title ?? file.Name.Replace(file.Extension, ""), fileInfo.Properties.Duration) {
                        BitRate = fileInfo.Properties.AudioBitrate,
                        Artist = fileInfo.Tag.FirstPerformer,
                        Album = fileInfo.Tag.Album,
                        Genre = fileInfo.Tag.FirstGenre,
                        Track = fileInfo.Tag.Track is var t and not 0 ? (int)t : null,
                        TrackCount = fileInfo.Tag.TrackCount is var tc and not 0 ? (int)tc : null,
                        Disc = fileInfo.Tag.Disc is var d and not 0 ? (int)d : null,
                        DiscCount = fileInfo.Tag.DiscCount is var dc and not 0 ? (int)dc : null,
                        Year = fileInfo.Tag.Year is var y and not 0 ? (int)y : null,
                        AlbumGain = fileInfo.Tag.ReplayGainAlbumGain is var ag and not double.NaN ? ag : null,
                        TrackGain = fileInfo.Tag.ReplayGainTrackGain is var tg and not double.NaN ? tg : null,
                    };
                    return result;
                }
                catch { }
            }
            return null;
        }
    }
}
