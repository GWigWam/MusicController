using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class AppSettingsViewModel : INotifyPropertyChanged {

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

        public uint SongTransitionDelay {
            get {
                return Settings.SongTransitionDelayMs;
            }
            set {
                Settings.SongTransitionDelayMs = value;
            }
        }

        public ObservableCollection<string> StartupFolders {
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

        public event PropertyChangedEventHandler PropertyChanged;

        public AppSettingsViewModel(AppSettings settings) {
            Settings = settings;
            Settings.Changed += Settings_Changed;

            StartupFolders = new ObservableCollection<string>();
            UpdateStartupFolders();

            SaveToDiskCommand = new AsyncCommand(
                (o) => Settings.WriteToDisc(),
                (o) => Settings.HasUnsavedChanges,
                (t) => {
                    if(t.IsFaulted)
                        Application.Current.Dispatcher.Invoke(() => new ExceptionWindow(t.Exception).Show());
                });
            Settings.Changed += (s, a) => ((AsyncCommand)SaveToDiskCommand).RaiseCanExecuteChanged();

            OpenFileLocationCommand = new RelayCommand((o) => System.Diagnostics.Process.Start("explorer.exe", $"//select, {Settings.FullFilePath}"));
        }

        public void AddStartupFolders(IEnumerable<string> paths) {
            foreach(var path in paths) {
                Settings.AddStartupFolder(path);
            }
            UpdateStartupFolders();
        }

        public void RemoveStartupFolders(IEnumerable<string> paths) {
            foreach(var path in paths) {
                Settings.RemoveStartupFolder(path);
            }
            UpdateStartupFolders();
        }

        private void UpdateStartupFolders() {
            StartupFolders.Clear();

            foreach(var sf in Settings.StartupFolders) {
                StartupFolders.Add(sf);
            }
        }

        private void Settings_Changed(object sender, SettingChangedEventArgs e) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.ChangedPropertyName));
        }
    }
}