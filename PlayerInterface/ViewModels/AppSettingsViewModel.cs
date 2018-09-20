using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Commands;
using PlayerInterface.ViewModels.FolderExplore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {
    public class AppSettingsViewModel : NotifyPropertyChanged {

        public AppSettings Settings { get; }

        public bool StartMinimized {
            get => Settings.StartMinimized;
            set => Settings.StartMinimized = value;
        }

        public bool EnableSpeech {
            get => Settings.EnableSpeech;
            set => Settings.EnableSpeech = value;
        }

        public uint SongTransitionDelay {
            get => Settings.SongTransitionDelayMs;
            set => Settings.SongTransitionDelayMs = value;
        }

        public uint ScreenOverlayShowTimeMs {
            get => Settings.ScreenOverlayShowTimeMs;
            set => Settings.ScreenOverlayShowTimeMs = value;
        }

        public uint ResetSentenceTimeMs {
            get => Settings.ResetSentenceTimeMs;
            set => Settings.ResetSentenceTimeMs = value;
        }

        public string Theme {
            get => Settings.Theme;
            set => Settings.Theme = value;
        }

        public ObservableCollection<ExplorerItem> LoadPaths { get; private set; }

        public IBaseCommand SaveToDiskCommand { get; private set; }

        public ICommand OpenFileLocationCommand { get; private set; }

        public AppSettingsViewModel(AppSettings settings) {
            Settings = settings;

            Settings.Changed += (s, a) => {
                RaisePropertyChanged(a.ChangedPropertyName);
                SaveToDiskCommand?.RaiseCanExecuteChanged();
            };
            Settings.Saved += (s, a) => SaveToDiskCommand.RaiseCanExecuteChanged();

            InitLoadPaths();
            SetupCommands();
        }

        private void SetupCommands() {
            SaveToDiskCommand = new AsyncCommand(
                o => Settings.WriteToDiscAsync(),
                o => Settings.HasUnsavedChanges,
                t => {
                    if(t.IsFaulted) {
                        Application.Current.Dispatcher.Invoke(() => new ExceptionWindow(t.Exception).Show());
                    }
                }
            );
            OpenFileLocationCommand = new RelayCommand(o => System.Diagnostics.Process.Start("explorer.exe", $"/select, {Settings.FullFilePath}"));
        }

        public void InitLoadPaths() {
            LoadPaths = new ObservableCollection<ExplorerItem>();

            foreach(var drive in DriveInfo.GetDrives().Where(di => di.IsReady)) {
                var folder = new ExplorerFolder(drive.Name, drive.Name.TrimEnd('\\'), Settings);
                LoadPaths.Add(folder);
            }

            var paths = Settings.StartupSongs
                .Distinct(ParentDirComparer.Default)
                .Select(sf => sf.Path)
                .Select(p => p.Split('\\', '/'));
            CheckPathBoxes(paths, LoadPaths);
        }

        private void CheckPathBoxes(IEnumerable<IEnumerable<string>> paths, IEnumerable<ExplorerItem> items) {
            var groups = paths
                .GroupBy(t => t.First());

            foreach(var group in groups) {
                var match = items.FirstOrDefault(i => i.Name == group.Key);
                if(match is ExplorerFile eFil) {
                    eFil.RaiseCheckStateChanged();
                } else if(match is ExplorerFolder eDir) {
                    CheckPathBoxes(group.Select(g => g.Skip(1)), eDir.Children);
                }
            }
        }

        private class ParentDirComparer : IEqualityComparer<SongFile> {
            public static ParentDirComparer Default = new ParentDirComparer();

            public bool Equals(SongFile x, SongFile y) =>
                x == null && y == null ? true :
                x == null || y == null ? false :
                GetHashCode(x) == GetHashCode(y);

            public int GetHashCode(SongFile obj) {
                if(obj == null) {
                    throw new ArgumentNullException(nameof(obj));
                }
                return new FileInfo(obj.Path).DirectoryName.ToLower().GetHashCode();
            }
        }
    }
}
