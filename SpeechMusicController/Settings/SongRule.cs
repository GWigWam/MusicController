using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpeechMusicController.Settings {

    [DebuggerDisplay("{Id}: {Type} {Parameters}")]
    internal class SongRule {
        public const string Seperator = ">>";

        public readonly int Id;

        public int SongHashCode { get; private set; }

        public SongRuleType Type { get; private set; }

        public string Parameters { get; set; }

        private SongRule() {
        }

        private SongRule(int _songHashCode, int _id = -1) {
            SongHashCode = _songHashCode;
            Parameters = string.Empty;
            if (_id == -1) {
                Id = SongRules.NextRuleId;
            } else {
                Id = _id;
            }
        }

        public static SongRule newExcludeRule(int hashCode) {
            var newRule = new SongRule(hashCode);
            newRule.Type = SongRuleType.Exclude;

            return newRule;
        }

        public static SongRule newNameChangeRule(int hashCode, string newName) {
            var newRule = new SongRule(hashCode);
            newRule.Type = SongRuleType.NameChange;
            newRule.Parameters = newName;

            return newRule;
        }

        public string ToSettingLine() {
            StringBuilder line = new StringBuilder(Id.ToString());
            line.Append(Seperator);
            switch (Type) {
                case SongRuleType.Exclude: {
                        line.Append("Exclude").Append(Seperator);
                        break;
                    }
                case SongRuleType.NameChange: {
                        line.Append("NameChange").Append(Seperator);
                        break;
                    }
            }
            line.AppendFormat("{0}{1}", Parameters, Seperator);
            line.Append(SongHashCode);

            return line.ToString();
        }

        public static bool TryParse(string input, out SongRule result) {
            try {
                var ruleParts = new Regex(Seperator).Split(input);

                result = new SongRule(Int32.Parse(ruleParts[3]), Int32.Parse(ruleParts[0]));

                if (ruleParts.Length == 4) {
                    switch (ruleParts[1]) {
                        case "Exclude": {
                                result.Type = SongRuleType.Exclude;
                                break;
                            }
                        case "NameChange": {
                                result.Type = SongRuleType.NameChange;
                                result.Parameters = ruleParts[2];
                                break;
                            }
                    }
                    return true;
                }
            } catch { }
            result = null;
            return false;
        }
    }

    internal enum SongRuleType {
        Exclude, NameChange
    }
}