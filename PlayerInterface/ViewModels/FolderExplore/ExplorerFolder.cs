using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels.FolderExplore {

    public class ExplorerFolder : ExplorerItem {

        public ExplorerFolder(string path, string name) : base(path, name) {
        }

        public override bool IsThreeState => true;

        public IEnumerable<ExplorerItem> Children => GetChildren();

        private bool? checkedState;

        public override bool? CheckedState {
            get { return checkedState; }
            set {
                checkedState = value;
                RaisePropertyChanged(nameof(CheckedState));

                if(value != null) {
                    ChildrenCache = null;
                    RaisePropertyChanged(nameof(Children));
                }
            }
        }

        private List<ExplorerItem> ChildrenCache;

        public override IEnumerable<string> GetCheckedPaths() {
            if(CheckedState == true) {
                yield return Path;
            } else if(CheckedState == null) {
                foreach(var path in Children.SelectMany(c => c.GetCheckedPaths())) {
                    yield return path;
                }
            }
        }

        private IEnumerable<ExplorerItem> GetChildren() {
            if(ChildrenCache == null) {
                ChildrenCache = new List<ExplorerItem>();
                var dirInfo = new DirectoryInfo(Path);
                foreach(var folder in dirInfo.GetDirectories()) {
                    ChildrenCache.Add(new ExplorerFolder(folder.FullName, folder.Name));
                }
                foreach(var file in dirInfo.GetFiles()) {
                    ChildrenCache.Add(new ExplorerFile(file.FullName, file.Name));
                }

                foreach(var child in ChildrenCache) {
                    if(CheckedState == true) {
                        child.CheckedState = true;
                    }

                    child.PropertyChanged += Child_PropertyChanged;
                }
            }
            return ChildrenCache;
        }

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(ExplorerItem.CheckedState)) {
                UpdateCheckedState();
            }
            RaisePropertyChanged(nameof(Children));
        }

        private void UpdateCheckedState() {
            if(Children.All(ei => ei.CheckedState == true)) {
                CheckedState = true;
            } else if(Children.All(ei => ei.CheckedState == false)) {
                CheckedState = false;
            } else {
                CheckedState = null;
            }
        }
    }
}