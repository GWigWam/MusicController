using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController.AppSettings {

    public abstract class SettingsFile {
        private static JsonSerializerSettings JSonSettings;

        [JsonIgnore]
        public bool HasUnsavedChanges {
            get; private set;
        }

        [JsonIgnore]
        public string FullFilePath {
            get; set;
        }

        public event EventHandler OnChange;

        [JsonConstructor]
        protected SettingsFile() {
            JSonSettings = new JsonSerializerSettings();
            JSonSettings.Formatting = Formatting.Indented;

            HasUnsavedChanges = true;
        }

        public SettingsFile(string filePath) : this() {
            if(IsPathValidRootedLocal(filePath)) {
                FullFilePath = filePath;
            } else {
                throw new ArgumentException("SettingsFile filePath is not valid");
            }
        }

        private bool IsPathValidRootedLocal(string path) {
            Uri pathUri;
            bool isValidUri = Uri.TryCreate(path, UriKind.Absolute, out pathUri);

            return isValidUri && pathUri != null && pathUri.IsLoopback;
        }

        public static T ReadSettingFile<T>(string filePath) where T : SettingsFile {
            try {
                string fileContent = File.ReadAllText(filePath);
                var read = JsonConvert.DeserializeObject<T>(fileContent, JSonSettings);
                read.FullFilePath = filePath;

                return read;
            } catch(FileNotFoundException) {
                return null;
            }
        }

        public void WriteToDisc(bool runAsync) {
            if(HasUnsavedChanges) {
                var writeTask = new Task(() => {
                    using(StreamWriter sw = new StreamWriter(new FileStream(FullFilePath, FileMode.Create))) {
                        string content = JsonConvert.SerializeObject(this, JSonSettings);
                        sw.Write(content);
                        HasUnsavedChanges = false;
                    }
                });

                if(runAsync) {
                    writeTask.Start();
                } else {
                    writeTask.RunSynchronously();
                }
            }
        }

        protected void AfterChange() {
            HasUnsavedChanges = true;
            OnChange?.Invoke(this, null);
        }
    }
}