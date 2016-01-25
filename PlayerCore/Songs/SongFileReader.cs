using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TagLib;

namespace PlayerCore.Songs {

    public static class SongFileReader {
        private static readonly Regex SongNameInfo = new Regex(@"^\s*(?<artist>.+?) - (?<title>.+?)(?<extension>\.[a-z]\S*)$");
        private static readonly Regex Parenthesis = new Regex(@"\(|\)");

        public static SongFile[] ReadFolderFiles(string path, string filter) {
            return YieldReadFolderFiles(path, filter).ToArray();
        }

        private static IEnumerable<SongFile> YieldReadFolderFiles(string path, string filter) {
            var dir = new DirectoryInfo(path);
            if(dir.Exists) {
                foreach(FileInfo f in dir.GetFiles(filter, SearchOption.AllDirectories)) {
                    var read = ReadFile(f);
                    if(read != null) {
                        yield return read;
                    }
                }
            }
        }

        public static SongFile ReadFile(FileInfo file) {
            TagLib.File fileInfo = null;
            try {
                fileInfo = TagLib.File.Create(file.FullName);
            } catch { }
            Match matchName = null;

            string title = fileInfo.Tag?.Title ?? (matchName ?? (matchName = SongNameInfo.Match(file.Name))).Groups?["title"]?.Value;
            string artist = fileInfo.Tag?.FirstPerformer ?? (matchName ?? (matchName = SongNameInfo.Match(file.Name))).Groups?["artist"]?.Value;
            string album = fileInfo.Tag?.Album;

            if(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(artist)) {
                //Remove parenthesis '(' & ')'
                title = Parenthesis.Replace(title, "").Trim();
                artist = Parenthesis.Replace(artist, "").Trim();
                album = album?.Trim();
                album = string.IsNullOrWhiteSpace(album) ? null : album;

                var songFile = new SongFile(file.FullName) {
                    Title = title,
                    Artist = artist,
                    Album = album,
                    Genre = fileInfo.Tag.FirstGenre,
                    Track = fileInfo.Tag.Track,
                    TrackCount = fileInfo.Tag.TrackCount,
                    Year = fileInfo.Tag.Year
                };

                if(fileInfo.Properties != null) {
                    songFile.BitRate = fileInfo.Properties.AudioBitrate;
                    songFile.TrackLength = fileInfo.Properties.Duration;
                }

                return songFile;
            }
            return null;
        }
    }
}