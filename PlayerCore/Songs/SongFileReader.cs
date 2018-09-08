using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayerCore.Songs {

    public static class SongFileReader {

        public static Song[] CreateSongs(AppSettings settings, params string[] paths) {
            return GetSongFiles(paths)
                .OrderBy(s => s.Artist)
                .ThenBy(s => s.Album)
                .ThenBy(s => s.Track)
                .Select(sf => new Song(sf, settings))
                .ToArray();
        }

        public static async Task<Song[]> CreateSongsAsync(AppSettings settings, params string[] paths) {
            return await Task.Run(() => CreateSongs(settings, paths)).ConfigureAwait(false);
        }

        private static IEnumerable<SongFile> GetSongFiles(params string[] paths) {
            SongFile ToSong(string path) => SongFile.Create(path);

            foreach (var path in paths) {
                if (File.Exists(path)) {
                    yield return ToSong(path);
                } else if (Directory.Exists(path)) {
                    foreach (var res in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Select(ToSong).Where(r => r != null)) {
                        yield return res;
                    }
                }
            }
        }
    }
}
