using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.Settings.Model {
    internal abstract class SongRule {
        public readonly string Title;
        public readonly string Artist;
        public readonly string Album;

        public SongRule(string title, string artist, string album) {
            Title = title ?? string.Empty;
            Artist = artist ?? string.Empty;
            Album = album ?? string.Empty;
        }
    }
}