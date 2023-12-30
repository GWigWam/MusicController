using PlayerCore.Persist;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels.FolderExplore {

    public abstract class ExplorerItem : NotifyPropertyChanged {
        
        public abstract bool? CheckedState { get; set; }
        public abstract bool IsThreeState { get; }

        public string Path { get; }
        public string Name { get; }

        protected AppSettings Settings { get; }

        public ExplorerItem(string path, string name, AppSettings settings) {
            Path = path;
            Name = name;
            Settings = settings;
        }
    }
}
