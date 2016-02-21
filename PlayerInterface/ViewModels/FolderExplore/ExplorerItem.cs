using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels.FolderExplore {

    public abstract class ExplorerItem : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract bool? CheckedState {
            get; set;
        }

        public abstract bool IsThreeState {
            get;
        }

        public string Path {
            get;
        }

        public string Name {
            get;
        }

        public ExplorerItem(string path, string name, bool? state = false) {
            Path = path;
            Name = name;
            CheckedState = state;
        }

        public abstract IEnumerable<string> GetCheckedPaths();

        public void RaisePropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}