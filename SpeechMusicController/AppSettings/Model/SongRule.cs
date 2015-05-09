using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.AppSettings.Model {
    public abstract class SongRule {
        public readonly string Title;
        public readonly string Artist;
        public readonly string Album;

        public readonly SongRuleType Type;

        public SongRule(string title, string artist, string album, SongRuleType type) {
            Title = title ?? string.Empty;
            Artist = artist ?? string.Empty;
            Album = album ?? string.Empty;

            Type = type;
        }
    }

    public enum SongRuleType {
        Exclude, NameChange
    }
}