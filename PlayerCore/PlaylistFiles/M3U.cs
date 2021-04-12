using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.PlaylistFiles
{
    public sealed class M3U
    {
        public string[] Paths { get; }

        public M3U(IEnumerable<string> paths)
        {
            Paths = paths.ToArray();
        }

        public static async Task<M3U> ReadAsync(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open);
            using var sr = new StreamReader(fs);
            var content = await sr.ReadToEndAsync().ConfigureAwait(false);

            static IEnumerable<string> parse(string fileContent)
            {
                var lines = fileContent.Split('\n')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t) && !t.StartsWith("#"));
                foreach(var line in lines)
                {
                    yield return line;
                }
            }

            var lines = parse(content);
            return new M3U(lines);
        }

        public async Task WriteAsync(string filePath, bool allowOverwrite)
        {
            using var fs = new FileStream(filePath, allowOverwrite ? FileMode.Create : FileMode.CreateNew);
            using var sr = new StreamWriter(fs);

            await sr.WriteLineAsync("#EXTM3U"); // File header

            foreach(var path in Paths)
            {
                //await sr.WriteLineAsync($"#EXTINF:{(int)path.TrackLength.TotalSeconds},{path.Artist} - {path.Title}"); // Info line
                await sr.WriteLineAsync(path); // Data line
            }
        }
    }
}
