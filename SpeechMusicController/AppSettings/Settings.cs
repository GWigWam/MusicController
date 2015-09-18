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
        public static readonly string FilePath = Path.GetFullPath("settings.json");

        [JsonProperty]
        private Dictionary<string, string> StringValues;

        public Settings() : base(FilePath) {
            StringValues = new Dictionary<string, string>() {
                ["MusicFolder"] = string.Empty,
                ["PlayerPath"] = string.Empty,
                ["SongRulesPath"] = Path.GetFullPath("SongRules.json")
            };
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