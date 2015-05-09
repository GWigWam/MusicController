using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.AppSettings.Model {
    public abstract class SongRule {
        public readonly SongAttributes Attributes;

        public readonly SongRuleType Type;

        public SongRule(string title, string artist, string album, SongRuleType type) {
            Attributes = new SongAttributes(title, artist, album);

            Type = type;
        }

        public override string ToString() {
            return string.Format("{0}: {1} - {2} ({3})", Type, Attributes.Title, Attributes.Artist, Attributes.Album);
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SongRuleType {
        Exclude, NameChange
    }
}