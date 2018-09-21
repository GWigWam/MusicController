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
            return sfs.Select(sf => new Song(sf, settings));
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

            return CreateSongFiles(GetAllFilePaths(paths));
        }
    }
}
