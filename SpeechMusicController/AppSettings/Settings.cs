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
        private static JsonSerializerSettings JSonSettings;

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
                            JSonSettings = new JsonSerializerSettings();
                            JSonSettings.Formatting = Formatting.Indented;

                            string fileContent = File.ReadAllText(FilePath);
                            instance = JsonConvert.DeserializeObject<Settings>(fileContent, JSonSettings);
                        } catch (JsonReaderException jre) {
                            System.Windows.Forms.MessageBox.Show("Invalid JSon in settings file!\n" + jre.ToString());
                            Environment.Exit(-1);
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
        private List<NameChangeRule> NameChangeRules;

        [JsonProperty]
        private List<ExcludeRule> ExcludeRules;

        public event Action OnRulesChanged;

        private Settings() {
            StringValues = new Dictionary<string, string>();
            NameChangeRules = new List<NameChangeRule>();
            ExcludeRules = new List<ExcludeRule>();

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
            if (rule is NameChangeRule) {
                NameChangeRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
                NameChangeRules.Add(rule as NameChangeRule);
            } else if (rule is ExcludeRule) {
                ExcludeRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
                ExcludeRules.Add(rule as ExcludeRule);
            }
            RulesChanged();
        }

        public void RemoveSongRule(SongRule rule) {
            if (rule is NameChangeRule) {
                NameChangeRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
            } else if (rule is ExcludeRule) {
                ExcludeRules.RemoveAll(sr => sr.Attributes == rule.Attributes && sr.Type == rule.Type);
            }
            RulesChanged();
        }

        public SongRule[] GetSongRules(bool getNameChangeRules, bool getExcludeRules) {
            return ExcludeRules.Concat<SongRule>(NameChangeRules).Where(s => {
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
                string content = JsonConvert.SerializeObject(instance, JSonSettings);
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