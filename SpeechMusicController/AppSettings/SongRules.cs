using Newtonsoft.Json;
using SpeechMusicController.AppSettings.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.AppSettings {

    public class SongRules : SettingsFile {

        [JsonProperty]
        private List<NameChangeRule> NameChangeRules;

        [JsonProperty]
        private List<ExcludeRule> ExcludeRules;

        [JsonConstructor]
        private SongRules() : base() {
            NameChangeRules = new List<NameChangeRule>();
            ExcludeRules = new List<ExcludeRule>();
        }

        public SongRules(string filePath) : base(filePath) {
            NameChangeRules = new List<NameChangeRule>();
            ExcludeRules = new List<ExcludeRule>();
        }

        public void AddSongRule(SongRule rule) {
            if(rule is NameChangeRule) {
                NameChangeRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
                NameChangeRules.Add(rule as NameChangeRule);
            } else if(rule is ExcludeRule) {
                ExcludeRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
                ExcludeRules.Add(rule as ExcludeRule);
            }
            AfterChange();
        }

        public void RemoveSongRule(SongRule rule) {
            if(rule is NameChangeRule) {
                NameChangeRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
            } else if(rule is ExcludeRule) {
                ExcludeRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
            }
            AfterChange();
        }

        public SongRule[] GetSongRules(bool getNameChangeRules, bool getExcludeRules) {
            return ExcludeRules.Concat<SongRule>(NameChangeRules).Where(s => {
                if(getNameChangeRules && s is NameChangeRule) {
                    return true;
                } else if(getExcludeRules && s is ExcludeRule) {
                    return true;
                } else {
                    return false;
                }
            }).ToArray();
        }
    }
}