using PlayerCore.PlaylistFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlayerCore.Songs
{
    public static class SongPathsHelper
    {
        public static IAsyncEnumerable<Song> CreateSongs(SongFileFactory factory, IEnumerable<string> paths)
        {
            IAsyncEnumerable<Song> CreateSongFiles(IEnumerable<string> filePaths)
            {
                return filePaths.ToAsyncEnumerable().SelectAwait(async p => await factory.CreateAsync(p)).Where(s => s != null);
            }

            IEnumerable<string> GetAllFilePaths(IEnumerable<string> fullPaths)
            {
                foreach(var fullPath in fullPaths)
                {
                    if(File.Exists(fullPath))
                    {
                        yield return fullPath;
                    }
                    else if(Directory.Exists(fullPath))
                    {
                        var ownDir = Directory.EnumerateFiles(fullPath).OrderBy(s => s);
                        var subDirs = GetAllFilePaths(Directory.EnumerateDirectories(fullPath)).OrderBy(s => s);
                        foreach (var innerPath in ownDir.Concat(subDirs))
                        {
                            yield return innerPath;
                        }
                    }
                }
            }

            IAsyncEnumerable<Song> CreateFromM3Us(IEnumerable<string> m3uPaths)
            {
                return m3uPaths.ToAsyncEnumerable()
                    .SelectAwait(async m3uPath => await M3U.ReadAsync(m3uPath))
                    .SelectMany(m3u => CreateSongFiles(m3u.Paths));
            }

            var m3uSplit = paths.ToLookup(p => p.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase));

            return CreateSongFiles(GetAllFilePaths(m3uSplit[false]))
                .Concat(CreateFromM3Us(m3uSplit[true]))
                .Distinct();
        }
    }
}
