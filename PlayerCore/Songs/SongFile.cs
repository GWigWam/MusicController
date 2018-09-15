using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayerCore.Songs {

    [DebuggerDisplay("{Title}: {Path})")]
    public class SongFile : IEqualityComparer<SongFile>, IEquatable<SongFile> {
        public string Album { get; private set; }
        public string Artist { get; private set; }
        public string Genre { get; private set; }
        public string Path { get; }
        public string Title { get; private set; }
        public uint Track { get; private set; }
        public uint TrackCount { get; private set; }
        public uint Year { get; private set; }
        public int BitRate { get; private set; }
        public TimeSpan TrackLength { get; private set; }

        private SongFile(string path) {
            Path = path;
        }

        public static SongFile Create(string filePath) {
            return Create(new FileInfo(filePath));
        }

        public static SongFile Create(FileInfo file) {
#warning TODO: Fix the .lnk mess
            if (file.Extension.Equals(".lnk", StringComparison.CurrentCultureIgnoreCase)) {
                file = LinkHelper.GetLinkTarget(file);
            }
            
            if (file == null || !SongPlayer.SupportedExtensions.Any(s => s.Equals(file.Extension, StringComparison.CurrentCultureIgnoreCase))) {
                return null;
            }

            TagLib.File fileInfo = null;
            try {
                fileInfo = TagLib.File.Create(file.FullName);
            } catch { }

            var title = fileInfo?.Tag?.Title ?? file.Name.Replace(file.Extension, "");
            var artist = string.IsNullOrEmpty(fileInfo?.Tag?.FirstPerformer?.Trim()) ? null : fileInfo.Tag.FirstPerformer.Trim();
            var album = string.IsNullOrEmpty(fileInfo?.Tag?.Album?.Trim()) ? null : fileInfo.Tag.Album.Trim();
            
            var songFile = new SongFile(file.FullName) {
                Title = title,
                Artist = artist,
                Album = album,
                Genre = fileInfo?.Tag?.FirstGenre,
                Track = fileInfo?.Tag?.Track ?? 0,
                TrackCount = fileInfo?.Tag?.TrackCount ?? 0,
                Year = fileInfo?.Tag?.Year ?? 0
            };

            if (fileInfo?.Properties != null) {
                songFile.BitRate = fileInfo.Properties.AudioBitrate;
                songFile.TrackLength = fileInfo.Properties.Duration;
            }

            return songFile;
        }

        public override bool Equals(object obj) => obj is SongFile sf ? Equals(sf) : false;

        public bool Equals(SongFile other) => Equals(this, other);

        public bool Equals(SongFile x, SongFile y) {
            return
                (x == null && y == null) ? true :
                (x == null || y == null) ? false :
                GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(SongFile obj) => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
       
        public override int GetHashCode() => Path.ToLower().GetHashCode();
    }
}
