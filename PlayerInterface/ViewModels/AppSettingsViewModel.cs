using PlayerCore.Settings;
using PlayerInterface.Commands;
using PlayerInterface.ViewModels.FolderExplore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class AppSettingsViewModel : NotifyPropertyChanged {
        public bool StartMinimized {
            get {
                return Settings.StartMinimized;
            }
            set {
                Settings.StartMinimized = value;
            }
        }

        public bool ShuffleOnStartup {
            get {
                return Settings.ShuffleOnStartup;
            }
            set {
                Settings.ShuffleOnStartup = value;
            }
        }

        public bool EnableSpeech {
            get {
                return Settings.EnableSpeech;
            }
            set {
                Settings.EnableSpeech = value;
            }
        }

        public uint SongTransitionDelay {
            get {
                return Settings.SongTransitionDelayMs;
            }
            set {
                Settings.SongTransitionDelayMs = value;
            }
        }

        public uint ScreenOverlayShowTimeMs {
            get {
                return Settings.ScreenOverlayShowTimeMs;
            }
            set {
                Settings.ScreenOverlayShowTimeMs = value;
            }
        }

        public uint ResetSentenceTimeMs {
            get {
                return Settings.ResetSentenceTimeMs;
            }
            set {
                Settings.ResetSentenceTimeMs = value;
            }
        }

        public ObservableCollection<ExplorerItem> LoadPaths {
            get; private set;
        }

        public ICommand SaveToDiskCommand {
            get; private set;
        }

        public ICommand OpenFileLocationCommand {
            get; private set;
        }

        private AppSettings Settings {
            get;
        }

        public AppSettingsViewModel(AppSettings settings) {
            Settings = settings;
            Settings.Changed += Settings_Changed;
            LoadPaths = new ObservableCollection<ExplorerItem>();

            InitLoadPaths();

            SaveToDiskCommand = new AsyncCommand(
                (o) => Settings.WriteToDisc(),
                (o) => Settings.HasUnsavedChanges,
                (t) => {
                    if(t.IsFaulted)
                        Application.Current.Dispatcher.Invoke(() => new ExceptionWindow(t.Exception).Show());
                });
            Settings.Changed += (s, a) => ((AsyncCommand)SaveToDiskCommand).RaiseCanExecuteChanged();
            Settings.Saved += (s, a) => ((AsyncCommand)SaveToDiskCommand).RaiseCanExecuteChanged();

            OpenFileLocationCommand = new RelayCommand(o => System.Diagnostics.Process.Start("explorer.exe", $"/select, {Settings.FullFilePath}"));
        }

        private void Settings_Changed(object sender, SettingChangedEventArgs e) {
            RaisePropertyChanged(e.ChangedPropertyName);
        }

        public void InitLoadPaths() {
            LoadPaths.Clear();

            foreach(var drive in DriveInfo.GetDrives().Where(di => di.IsReady)) {
                var folder = new ExplorerFolder(drive.Name, drive.Name);
                LoadPaths.Add(folder);
            }

            foreach(var path in Settings.StartupFolders) {
                foreach(var drive in LoadPaths) {
                    if(TryCheckPath(path.Split('\\', '/').Skip(1), drive)) {
                        break;
                    }
                }
            }
        }

        private bool TryCheckPath(IEnumerable<string> path, ExplorerItem item) {
            var curPathLength = path.Count();
            if(curPathLength == 1 && item is ExplorerFile) {
                if(path.First() == item.Name) {
                    item.CheckedState = true;
                    return true;
                } else {
                    return false;
                }
            } else if(curPathLength >= 1 && item is ExplorerFolder) {
                var found = (item as ExplorerFolder).Children.FirstOrDefault(i => i.Name == path.First());
                if(found != null) {
                    if(curPathLength == 1) {
                        found.CheckedState = true;
                        return true;
                    } else {
                        return TryCheckPath(path.Skip(1), found);
                    }
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        internal void TreeView_LostFocus(object sender, RoutedEventArgs e) {
            var newCol = new List<string>();
            foreach(var item in LoadPaths) {
                newCol.AddRange(item.GetCheckedPaths());
            }
            Settings.ClearStarupFolders();
            Settings.AddStartupFolders(newCol);
        }
    }
}