using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public class AppSettings : SettingsFile {
        private bool startMinimized = true;

        [JsonProperty]
        public bool StartMinimized {
            get { return startMinimized; }
            set {
                if(value != startMinimized) {
                    startMinimized = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartMinimized)));
                }
            }
        }

        private uint songTransitionDelayMs = 2000;

        [JsonProperty]
        public uint SongTransitionDelayMs {
            get { return songTransitionDelayMs; }
            set {
                if(value != songTransitionDelayMs) {
                    songTransitionDelayMs = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(SongTransitionDelayMs)));
                }
            }
        }

        private bool shuffleOnStartup = true;

        [JsonProperty]
        public bool ShuffleOnStartup {
            get { return shuffleOnStartup; }
            set {
                if(value != shuffleOnStartup) {
                    shuffleOnStartup = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(SongTransitionDelayMs)));
                }
            }
        }

        private bool enableSpeech = true;

        [JsonProperty]
        public bool EnableSpeech {
            get { return enableSpeech; }
            set {
                if(value != enableSpeech) {
                    enableSpeech = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(EnableSpeech)));
                }
            }
        }

        private uint screenOverlayShowTimeMs = 1200;

        [JsonProperty]
        public uint ScreenOverlayShowTimeMs {
            get { return screenOverlayShowTimeMs; }
            set {
                if(value != screenOverlayShowTimeMs) {
                    screenOverlayShowTimeMs = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(ScreenOverlayShowTimeMs)));
                }
            }
        }

        private uint resetSentenceTimeMs = 6000;

        [JsonProperty]
        public uint ResetSentenceTimeMs {
            get { return resetSentenceTimeMs; }
            set {
                if(value != resetSentenceTimeMs) {
                    resetSentenceTimeMs = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(ResetSentenceTimeMs)));
                }
            }
        }

        private int windowHeight = 390;

        [JsonProperty]
        public int WindowHeight {
            get { return windowHeight; }
            set {
                if(value != windowHeight) {
                    windowHeight = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(WindowHeight)));
                }
            }
        }

        private int windowWidth = 300;

        [JsonProperty]
        public int WindowWidth {
            get { return windowWidth; }
            set {
                if(value != windowWidth) {
                    windowWidth = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(WindowWidth)));
                }
            }
        }

        [JsonProperty]
        private HashSet<string> startupFolders {
            get; set;
        } = new HashSet<string>();

        private List<SongStats> statistics = new List<SongStats>();

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        protected List<SongStats> Statistics {
            get { return statistics; }
            set {
                statistics = value;
                foreach(var s in statistics) {
                    s.PropertyChanged += SongStat_PropertyChanged;
                }
            }
        }

        [JsonIgnore]
        public IEnumerable<SongStats> SongStats => Statistics;

        [JsonIgnore]
        public IEnumerable<string> StartupFolders => startupFolders;

        public AppSettings(string filePath) : base(filePath) {
            // ---
        }

        [JsonConstructor]
        protected AppSettings() : base() {
        }

        public void AddStartupFolder(string path) {
            startupFolders.Add(path);
            RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartupFolders)));
        }

        public void AddStartupFolders(IEnumerable<string> paths) {
            foreach(var path in paths) {
                startupFolders.Add(path);
            }
            RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartupFolders)));
        }

        public void RemoveStartupFolder(string path) {
            startupFolders.Remove(path);
            RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartupFolders)));
        }

        public void ClearStarupFolders() {
            startupFolders.Clear();
            RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartupFolders)));
        }

        public void AddSongStats(params SongStats[] stats) {
            Statistics.AddRange(stats);
            foreach(var s in stats) {
                s.PropertyChanged += SongStat_PropertyChanged;
            }
            RaiseChanged(nameof(SongStats));
        }

        private void SongStat_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            RaiseChanged(nameof(SongStats));
        }
    }
}