using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels.FolderExplore {

    public class ExplorerFolder : ExplorerItem {
        
        public override bool IsThreeState => true;

        public IEnumerable<ExplorerItem> Children => ChildrenCache.Value;

        private bool? checkedState = false;
        public override bool? CheckedState {
            get => checkedState;
            set => TryChangeCheckState(value);
        }

        private Lazy<ExplorerItem[]> ChildrenCache { get; }

        public ExplorerFolder(string path, string name, AppSettings settings) : base(path, name, settings) {
            ChildrenCache = new Lazy<ExplorerItem[]>(() => SetupChildren().ToArray());
        }

        private IEnumerable<ExplorerItem> SetupChildren() {
            IEnumerable<ExplorerItem> getChildren() {
                var dirInfo = new DirectoryInfo(Path);
                foreach (var folder in dirInfo.EnumerateDirectories().Where(di => (di.Attributes & (FileAttributes.System | FileAttributes.Hidden)) == 0)) {
                    yield return new ExplorerFolder(folder.FullName, folder.Name, Settings);
                }
                foreach (var file in dirInfo.EnumerateFiles()) {
                    yield return new ExplorerFile(file.FullName, file.Name, Settings);
                }
            }

            foreach (var child in getChildren()) {
                if (CheckedState == true) {
                    child.CheckedState = true;
                }
                child.PropertyChanged += Child_PropertyChanged;
                yield return child;
            }
        }

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(ExplorerItem.CheckedState)) {
                UpdateOwnCheckState();
            }
        }

        private void UpdateOwnCheckState() {
            if (Children.All(ei => ei.CheckedState == true)) {
                checkedState = true;
            } else if (Children.All(ei => ei.CheckedState == false)) {
                checkedState = false;
            } else {
                checkedState = null;
            }
            RaisePropertyChanged(nameof(CheckedState));
        }

        internal bool TryChangeCheckState(bool? newState) {
            if (newState == checkedState) {
                return true;
            }

            if (newState is null) {
                checkedState = null;
                RaisePropertyChanged(nameof(CheckedState));
                return true;
            }

            var (suc, children) = TryGetChildren();
            if (suc) {
                foreach (var child in children) {
                    child.CheckedState = newState;
                }
            }
            return suc;
        }

        private (bool success, IEnumerable<ExplorerFile> files) TryGetChildren(int dirsChecked = 0) {
            const int MaxDirs = 100;
            if (dirsChecked >= MaxDirs) {
                return (false, null);
            }

            var res = new List<ExplorerFile>();
            foreach (var childDir in Children.OfType<ExplorerFolder>()) {
                dirsChecked++;
                var (cSuc, cFiles) = childDir.TryGetChildren(dirsChecked);
                if (!cSuc) {
                    return (false, null);
                }
                res.AddRange(cFiles);
            }
            res.AddRange(Children.OfType<ExplorerFile>());
            return (true, res);
        }
    }
}
