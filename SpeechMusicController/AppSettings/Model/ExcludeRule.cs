using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.AppSettings.Model {
    internal class ExcludeRule : SongRule {

        public ExcludeRule(SongAttributes attributes)
            : base(attributes, SongRuleType.Exclude) {
        }
    }
}