using PlayerCore.PlaylistFiles;
using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayerCore.Songs {

    public static class SongPathsHelper {

        public static IEnumerable<Song> CreateSongs(AppSettings settings, params string[] paths) {
            var sfs = GetSongFiles(paths);
            return sfs.Select(sf => new Song(sf));
        }

        private static IEnumerable<SongFile> GetSongFiles(params string[] paths) {
            IEnumerable<SongFile> CreateSongFiles(IEnumerable<string> filePaths) {
                foreach(var filePath in filePaths) {
                    if(SongFile.TryCreate(filePath, out var res)) {
                        yield return res;
                    }
                }
            }

            IEnumerable<string> GetAllFilePaths(IEnumerable<string> fullPaths) {
                foreach(var fullPath in fullPaths) {
                    if(File.Exists(fullPath)) {
                        yield return fullPath;
                    } else if(Directory.Exists(fullPath)) {
                        foreach(var innerPath in Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories)) {
                            yield return innerPath;
                        }
                    }
                }
            }

            IEnumerable<SongFile> CreateFromM3Us(IEnumerable<string> m3uPaths) {
                foreach(var m3u in m3uPaths) {
                    if(File.Exists(m3u)) {
                        var res = M3U.ReadAsync(m3u).Result; //TODO: Use async enumerables
                        foreach(var songFile in res.Files) {
                            yield return songFile;
                        }
                    }
                }
            }

            var m3uSplit = paths.ToLookup(p => p.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase));

            return CreateSongFiles(GetAllFilePaths(m3uSplit[false]))
                .Concat(CreateFromM3Us(m3uSplit[true]));
        }
    }
}
