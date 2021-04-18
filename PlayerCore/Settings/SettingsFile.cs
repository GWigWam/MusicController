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

        private Task WritingTask { get; set; }

        [JsonConstructor]
        protected SettingsFile() { }

        public static T ReadSettingFile<T>(string filePath) where T : SettingsFile {
            var task = Task.Run(() => ReadSettingFileAsync<T>(filePath));
            return task.Result;
        }

        public static async Task<T> ReadSettingFileAsync<T>(string filePath) where T : SettingsFile
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                var fileContent = await sr.ReadToEndAsync();

                var read = JsonConvert.DeserializeObject<T>(fileContent, JSonSettings);
                read.FullFilePath = filePath;
                await read.AfterRead();
                read.HasUnsavedChanges = false;

                return read;
            }
            catch(JsonReaderException jre)
            {
                throw new Exception($"Invalid json encountered while trying to create {typeof(T).Name} from:\n{filePath}\n{jre}", jre);
            }
            catch(FileNotFoundException)
            {
                return null;
            }
        }

        public void WriteToDisc() {
            var task = Task.Run(WriteToDiscAsync);
            task.Wait();
        }

        public async Task WriteToDiscAsync() {
            if(HasUnsavedChanges) {
                try {
                    WritingTask = WritingTask ?? WriteToDiskInternalAsync();
                    await WritingTask;
                    HasUnsavedChanges = false;
                } catch(Exception) {
                } finally {
                    WritingTask = null;
                }
            }
            RaiseSaved();
        }

        protected virtual async Task WriteToDiskInternalAsync() {
            //Write to .writing file
            var writingPath = $"{FullFilePath}.writing";
            using(StreamWriter sw = new StreamWriter(new FileStream(writingPath, FileMode.Create))) {
                string content = JsonConvert.SerializeObject(this, JSonSettings);
                await sw.WriteAsync(content);
            }

            //After writing is finished delete old file and remove .writing from the filename
            if(File.Exists(writingPath)) {
                File.Delete(FullFilePath);
                File.Move(writingPath, FullFilePath);
            }
        }

        protected void RaiseChanged(SettingChangedEventArgs args) {
            HasUnsavedChanges = true;
            Changed?.Invoke(this, args);
        }

        protected void RaiseChanged(string propertyName) {
            var prop = typeof(AppSettings).GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if(prop == null) {
                throw new ArgumentException("Invalid property name");
            }
            var args = new SettingChangedEventArgs(prop);
            RaiseChanged(args);
        }

        protected void RaiseSaved() => Saved?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Called after method ReadSettingFile has deserialized a file
        /// </summary>
        protected virtual Task AfterRead() => Task.CompletedTask;

        private bool IsPathValidRootedLocal(string path) {
            Uri pathUri;
            bool isValidUri = Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out pathUri);

            return isValidUri && pathUri != null;
        }
    }
}
