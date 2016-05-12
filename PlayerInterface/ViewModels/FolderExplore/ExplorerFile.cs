using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels.FolderExplore {

    public class ExplorerFile : ExplorerItem {

        public ExplorerFile(string path, string name, bool state = false) : base(path, name, state) {
        }

        public override bool IsThreeState => false;

        private bool checkedState;

        public override bool? CheckedState {
            get { return checkedState; }
            set {
                if(checkedState != value) {
                    checkedState = value ?? false;
                    RaisePropertyChanged();
                }
            }
        }

        public override IEnumerable<string> GetCheckedPaths() {
            if(CheckedState == true) {
                yield return Path;
            }
        }
    }
}