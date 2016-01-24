using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Songs {

    public class SongFile {
        public string Album { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public string Path { get; }
        public string Title { get; set; }
        public uint Track { get; set; }
        public uint TrackCount { get; set; }
        public uint Year { get; set; }
        public int BitRate { get; set; }
        public TimeSpan TrackLength { get; set; }

        public SongFile(string path) {
            Path = path;
        }
    }
}