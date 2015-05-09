using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.Settings.Model {
    internal class NameChangeRule : SongRule {
        public readonly string NewName;

        public NameChangeRule(string title, string artist, string album, string newName)
            : base(title, artist, album) {
            if (string.IsNullOrEmpty(newName)) {
                throw new ArgumentException("NewName");
            }

            NewName = newName;
        }
    }
}