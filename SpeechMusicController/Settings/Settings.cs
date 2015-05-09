using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.Settings {
    [JsonObject(Description = "SpeechMusicController's settings")]
    public class Settings {
        private const string FilePath = "settings.json";
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

        private Settings() {
            StringValues = new Dictionary<string, string>();
            StringValues["MusicFolder"] = string.Empty;
            StringValues["PlayerPath"] = string.Empty;
        }

        public void SetSetting(string name, string value) {
            StringValues[name] = value;
            WriteToDisc();
        }

        public void WriteToDisc() {
            using (StreamWriter sw = new StreamWriter(new FileStream(FilePath, FileMode.Create))) {
                string content = JsonConvert.SerializeObject(instance);
                sw.Write(content);
            }
        }
    }
}