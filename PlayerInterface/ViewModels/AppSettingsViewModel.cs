using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ICommand SaveToDiskCommand {
            get; private set;
        }

        private AppSettings Settings {
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppSettingsViewModel(AppSettings settings) {
            Settings = settings;
            Settings.Changed += Settings_Changed;

            SaveToDiskCommand = new RelayCommand((o) => Settings.WriteToDisc(true), (o) => Settings.HasUnsavedChanges);
        }

        private void Settings_Changed(object sender, SettingChangedEventArgs e) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.ChangedPropertyName));
        }
    }
}