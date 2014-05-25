using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {
    class Song {
        public string SongName;
        public string BandName;

        public string FilePath;

        public Song(string _SongName, string _BandName, string _FilePath) {
            SongName = _SongName;
            BandName = _BandName;
            FilePath = _FilePath;
        }
    }
}
