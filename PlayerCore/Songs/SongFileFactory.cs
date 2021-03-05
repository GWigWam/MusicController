using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Songs {
    internal static class SongFileFactory {

        private static ConcurrentDictionary<string, SongFile> Cache = new ConcurrentDictionary<string, SongFile>();

        public static bool TryGet(string filePath, out SongFile result) {
            filePath = filePath.ToLower();
            if(Cache.TryGetValue(filePath, out result)) {
                return true;
            } else {
                if(TryCreate(filePath, out result)) {
                    Cache.TryAdd(filePath, result);
                    return true;
                } else {
                    return false;
                }
            }
        }

        private static bool TryCreate(string filePath, out SongFile result) {
            result = null;

            if(!File.Exists(filePath)) {
                return false;
            }

            var file = new FileInfo(filePath);
            if(!SongPlayer.SupportedExtensions.Any(s => s.Equals(file.Extension, StringComparison.CurrentCultureIgnoreCase))) {
                return false;
            }

            try {
                var fileInfo = TagLib.File.Create(file.FullName);

                var title = fileInfo?.Tag?.Title ?? file.Name.Replace(file.Extension, "");
                var artist = string.IsNullOrEmpty(fileInfo?.Tag?.FirstPerformer?.Trim()) ? null : fileInfo.Tag.FirstPerformer.Trim();
                var album = string.IsNullOrEmpty(fileInfo?.Tag?.Album?.Trim()) ? null : fileInfo.Tag.Album.Trim();

                result = new SongFile(
                    path: filePath,
                    title: title,
                    artist: artist,
                    album: album,
                    genre: fileInfo?.Tag?.FirstGenre,
                    track: fileInfo?.Tag?.Track ?? 0,
                    trackCount: fileInfo?.Tag?.TrackCount ?? 0,
                    disc: fileInfo?.Tag?.Disc ?? 0,
                    discCount: fileInfo?.Tag?.DiscCount ?? 0,
                    year: fileInfo?.Tag?.Year ?? 0,
                    bitRate: fileInfo?.Properties?.AudioBitrate ?? -1,
                    trackLength: fileInfo?.Properties?.Duration ?? TimeSpan.Zero
                );

                return true;
            } catch {
                return false;
            }
        }
    }
}
