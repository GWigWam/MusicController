using Newtonsoft.Json;
using SpeechMusicController.AppSettings.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.AppSettings {
    [JsonObject(Description = "SpeechMusicController's settings")]
    public class Settings {
        private const string FilePath = "settings.json";

        [JsonIgnore]
        public string FullFilePath {
            get {
                var path = Path.GetFullPath(FilePath);
                return path;
            }
        }

        private static Settings instance;

        [JsonIgnore]
        public static Settings Instance {
            get {
                if (instance == null) {
                    if (File.Exists(FilePath)) {
                        try {
                            string fileContent = File.ReadAllText(FilePath);
                            instance = JsonConvert.DeserializeObject<Settings>(fileContent);
                        } catch {
                            File.Delete(FilePath);
                            instance = new Settings();
                        }
                    } else {
                        instance = new Settings();
                    }
                }
                return instance;
            }
        }

        [JsonProperty]
        private Dictionary<string, string> StringValues;

        [JsonProperty]
        private List<SongRule> SongRules;

        public event Action OnRulesChanged;

        private Settings() {
            StringValues = new Dictionary<string, string>();
            SongRules = new List<SongRule>();

            StringValues["MusicFolder"] = string.Empty;
            StringValues["PlayerPath"] = string.Empty;
        }

        public void SetSetting(string name, string value) {
            StringValues[name] = value;
            RulesChanged();
        }

        public string GetSetting(string name) {
            if (!string.IsNullOrEmpty(name) && StringValues.ContainsKey(name)) {
                return StringValues[name];
            } else {
                return null;
            }
        }

        public string[] GetAllSettingNames() {
            return StringValues.Keys.ToArray();
        }

        public void AddSongRule(SongRule rule) {
            SongRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
            SongRules.Add(rule);
            RulesChanged();
        }

        public void RemoveSongRule(SongRule rule) {
            SongRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
            RulesChanged();
        }

        public SongRule[] GetSongRules(bool getNameChangeRules, bool getExcludeRules) {
            return SongRules.Where(s => {
                if (getNameChangeRules && s is NameChangeRule) {
                    return true;
                } else if (getExcludeRules && s is ExcludeRule) {
                    return true;
                } else {
                    return false;
                }
            }).ToArray();
        }

        public void WriteToDisc() {
            using (StreamWriter sw = new StreamWriter(new FileStream(FilePath, FileMode.Create))) {
                string content = JsonConvert.SerializeObject(instance);
                sw.Write(content);
            }
        }

        private void RulesChanged() {
            if (OnRulesChanged != null) {
                OnRulesChanged();
            }
        }
    }
}