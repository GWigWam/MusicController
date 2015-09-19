using Newtonsoft.Json;
using SpeechMusicController.AppSettings.Model;
using System;
using System.Collections;
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
        private Dictionary<string, object> NamedValueCollection;

        [JsonConstructor]
        private Settings() {
            NamedValueCollection = new Dictionary<string, object>();
        }

        public Settings(bool initValues = true) : base(FilePath) {
            NamedValueCollection = new Dictionary<string, object>();

            if(initValues) {
                SetSetting("MusicFolder", string.Empty);
                SetSetting("PlayerPath", string.Empty);
                SetSetting("SongRulesPath", Path.GetFullPath("SongRules.json"));
                SetSetting("MessageOverlayVisibleTimeMs", 500L);
            }
        }

        public string[] GetAllSettingNames() => NamedValueCollection.Keys.ToArray();

        public bool TryGetSetting<T>(string name, out T found) {
            if(NamedValueCollection.ContainsKey(name) && NamedValueCollection[name] is T) {
                found = (T)NamedValueCollection[name];
                return true;
            } else {
                found = default(T);
                return false;
            }
        }

        public object GetSetting(string name) {
            if(NamedValueCollection.ContainsKey(name)) {
                return NamedValueCollection[name];
            } else {
                return null;
            }
        }

        public void SetSetting<T>(string name, T value) {
            if(value != null && NamedValueCollection.ContainsKey(name)) {
                if(NamedValueCollection[name].GetType() == value.GetType()) {
                    NamedValueCollection[name] = value;
                } else {
                    throw new ArgumentException($"Cannot set {name} to {value} because it has type {NamedValueCollection[name].GetType().Name} and {value} is {value.GetType().Name}");
                }
            } else {
                NamedValueCollection[name] = value;
            }

            AfterChange();
        }
    }
}