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
    public class Settings : SettingsFile {
        private static readonly string FilePath = Path.GetFullPath("settings.json");

        private static Settings instance;

        [JsonProperty]
        private Dictionary<string, string> StringValues;

        private Settings() : base(FilePath) {
            StringValues = new Dictionary<string, string>() {
                ["MusicFolder"] = string.Empty,
                ["PlayerPath"] = string.Empty,
                ["SongRulesPath"] = Path.GetFullPath("SongRules.json")
            };
        }

        [JsonIgnore]
        public static Settings Instance {
            get {
                if(instance == null) {
                    if(File.Exists(FilePath)) {
                        try {
                            instance = ReadSettingFile<Settings>(FilePath);

                            if(instance == null) {
                                File.Delete(FilePath);
                                instance = new Settings();
                            }
                        } catch(JsonReaderException jre) {
                            System.Windows.Forms.MessageBox.Show("Invalid Json in settings file!\n" + jre.ToString());
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

        public string[] GetAllSettingNames() => StringValues.Keys.ToArray();

        public string GetSetting(string name) {
            if(!string.IsNullOrEmpty(name) && StringValues.ContainsKey(name)) {
                return StringValues[name];
            } else {
                return null;
            }
        }

        public void SetSetting(string name, string value) {
            StringValues[name] = value;
            AfterChange();
        }
    }
}