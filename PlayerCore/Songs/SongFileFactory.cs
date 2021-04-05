using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Songs {
    internal static class SongFileFactory {

        private static readonly ConcurrentDictionary<string, SongFile> Cache = new ConcurrentDictionary<string, SongFile>();

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

            var file = new FileInfo(filePath);
            if(!file.Exists) {
                return false;
            }
            else if(!SongPlayer.SupportedExtensions.Any(s => s.Equals(file.Extension, StringComparison.CurrentCultureIgnoreCase))) {
                return false;
            }

            try {
                var fileInfo = TagLib.File.Create(file.FullName);

                var title = fileInfo.Tag.Title ?? file.Name.Replace(file.Extension, "");
                result = new SongFile(
                    path: filePath,
                    title: title,
                    artist: fileInfo.Tag.FirstPerformer,
                    album: fileInfo.Tag.Album,
                    genre: fileInfo.Tag.FirstGenre,
                    track: (int)fileInfo.Tag.Track,
                    trackCount: (int)fileInfo.Tag.TrackCount,
                    disc: (int)fileInfo.Tag.Disc,
                    discCount: (int)fileInfo.Tag.DiscCount,
                    year: (int)fileInfo.Tag.Year,
                    bitRate: fileInfo.Properties.AudioBitrate,
                    albumGain: fileInfo.Tag.ReplayGainAlbumGain is var ag and not double.NaN ? ag : null,
                    trackGain: fileInfo.Tag.ReplayGainTrackGain is var tg and not double.NaN ? tg : null,
                    trackLength: fileInfo.Properties?.Duration ?? TimeSpan.Zero
                );

                return true;
            } catch {
                return false;
            }
        }
    }
}
