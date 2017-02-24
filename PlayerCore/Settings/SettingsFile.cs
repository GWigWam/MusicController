using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public abstract class SettingsFile {
        private static JsonSerializerSettings JSonSettings;

        [JsonIgnore]
        public string FullFilePath {
            get; private set;
        }

        [JsonIgnore]
        public bool HasUnsavedChanges {
            get; protected set;
        } = false;

        public event EventHandler<SettingChangedEventArgs> Changed;

        public event EventHandler Saved;

        static SettingsFile() {
            JSonSettings = new JsonSerializerSettings() {
                Formatting = Formatting.Indented,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
        }

        public SettingsFile(string filePath) : this() {
            if(IsPathValidRootedLocal(filePath)) {
                FullFilePath = filePath;
                HasUnsavedChanges = true;
            } else {
                throw new ArgumentException("SettingsFile filePath is not valid");
            }
        }

        [JsonConstructor]
        protected SettingsFile() {
        }

        public static T ReadSettingFile<T>(string filePath) where T : SettingsFile {
            try {
                string fileContent = File.ReadAllText(filePath);
                var read = JsonConvert.DeserializeObject<T>(fileContent, JSonSettings);
                if(read != null) {
                    read.FullFilePath = filePath;

                    read.AfterRead();

                    read.HasUnsavedChanges = false;
                    return read;
                } else {
                    throw new Exception($"Could not read {typeof(T).Name} from\n{filePath}\nFile is probably empty"); //TODO: Userfriendly exception
                }
            } catch(JsonReaderException jre) {
                throw new Exception($"Invalid json encountered while trying to create {typeof(T).Name} from:\n{filePath}\n{jre}"); //TODO: Userfriendly exception
            } catch(FileNotFoundException) {
                return null;
            }
        }

        private static object WriteLock = new object();

        public void WriteToDisc() {
            lock(WriteLock) {
                if(HasUnsavedChanges) {
                    HasUnsavedChanges = false;
                    //Write to .writing file
                    var writingPath = $"{FullFilePath}.writing";
                    using(StreamWriter sw = new StreamWriter(new FileStream(writingPath, FileMode.Create))) {
                        string content = JsonConvert.SerializeObject(this, JSonSettings);
                        sw.Write(content);
                    }

                    //After writing is finished delete old file and remove .writing from the filename
                    if(File.Exists(writingPath)) {
                        File.Delete(FullFilePath);
                        File.Move(writingPath, FullFilePath);
                    }
                }
            }
            RaiseSaved();
        }

        protected void RaiseChanged(SettingChangedEventArgs args) {
            HasUnsavedChanges = true;
            Changed?.Invoke(this, args);
        }

        protected void RaiseChanged(string propertyName) {
            var prop = typeof(AppSettings).GetProperty(propertyName);
            if(prop == null) {
                throw new ArgumentException("Invalid property name");
            }
            var args = new SettingChangedEventArgs(prop);
            RaiseChanged(args);
        }

        protected void RaiseSaved() {
            Saved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called after method ReadSettingFile has deserialized a file
        /// </summary>
        protected virtual void AfterRead() {
            //Empty
        }

        private bool IsPathValidRootedLocal(string path) {
            Uri pathUri;
            bool isValidUri = Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out pathUri);

            return isValidUri && pathUri != null;
        }
    }
}