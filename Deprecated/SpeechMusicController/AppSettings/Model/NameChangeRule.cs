using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.AppSettings.Model {

    internal class NameChangeRule : SongRule {
        public readonly string NewName;

        public NameChangeRule(SongAttributes attributes, string newName)
            : base(attributes, SongRuleType.NameChange) {
            if(string.IsNullOrEmpty(newName)) {
                throw new ArgumentException("NewName");
            }

            NewName = newName;
        }

        public override string ToString() => $"{base.ToString()} --> {NewName}";
    }
}