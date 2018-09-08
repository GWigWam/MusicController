using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.M3U {
    public sealed class M3U {

        public SongFile[] Files { get; }

        public M3U(IEnumerable<SongFile> files) {
            Files = files.ToArray();
        }

        public static async Task<M3U> ReadAsync(string filePath) {
            string content;
            using (var fs = new FileStream(filePath, FileMode.Open))
            using (var sr = new StreamReader(fs)) {
                content = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            IEnumerable<string> Parse(string s) {
                foreach (var line in s.Split('\n')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t) && !t.StartsWith("#"))) {
                    yield return line;
                }
            }

            var lines = Parse(content).ToArray();
            var songFiles = lines.Select(s => SongFile.Create(s)).ToArray();
            return new M3U(songFiles);
        }

        public async Task WriteAsync(string filePath, bool allowOverwrite) {
            using (var fs = new FileStream(filePath, allowOverwrite ? FileMode.Create : FileMode.CreateNew))
            using (var sr = new StreamWriter(fs)) {
                await sr.WriteLineAsync("#EXTM3U"); // File header

                foreach (var sf in Files) {
                    await sr.WriteLineAsync($"#EXTINF:{(int)sf.TrackLength.TotalSeconds},{sf.Artist} - {sf.Title}"); // Info line
                    await sr.WriteLineAsync(sf.Path); // Data line
                }
            }
        }
    }
}
