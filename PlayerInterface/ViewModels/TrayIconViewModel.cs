using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {
    public class TrayIconViewModel : NotifyPropertyChanged {
        private const string DefaultToolTipText = "MusicController";
        private string toolTipText = DefaultToolTipText;

        public string ToolTipText {
            get { return toolTipText; }
            set {
                if(string.IsNullOrEmpty(value)) {
                    toolTipText = DefaultToolTipText;
                } else if(value != toolTipText) {
                    toolTipText = value;
                }
                RaisePropertyChanged();
            }
        }

        public ICommand SmallPlayer {
            get; set;
        }

        public ICommand FullPlayer {
            get; set;
        }

        public ICommand Quit {
            get; set;
        }
    }
}