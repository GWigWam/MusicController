using PlayerCore.Persist;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels.FolderExplore {

    public class ExplorerFile : ExplorerItem {

        public override bool IsThreeState => false;

        private bool _checkState;
        public override bool? CheckedState {
            get => _checkState;
            set {
                if (value is bool isStartupSong && isStartupSong != CheckedState) {
                    if (isStartupSong) {
                        Settings.AddStartupSong(Path);
                    } else {
                        Settings.RemoveStartupSong(Path);
                    }
                    _checkState = isStartupSong;
                    RaisePropertyChanged();
                }
            }
        }

        public ExplorerFile(string path, string name, AppSettings settings) : base(path, name, settings) {
            _checkState = Settings.IsStartupSong(Path);
            settings.StartupSongsChanged += (s, a) => {
                if (a.SongPath.Equals(Path, StringComparison.OrdinalIgnoreCase)) {
                    _checkState = a.IsStartupSong;
                    RaisePropertyChanged(nameof(CheckedState));
                }
            };
        }

        public void RaiseCheckStateChanged() => RaisePropertyChanged(nameof(CheckedState));
    }
}
