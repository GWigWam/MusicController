using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class TrayIconViewModel : INotifyPropertyChanged {
        private const string DefaultToolTipText = "SpeechMusicController";
        private string toolTipText = DefaultToolTipText;

        public string ToolTipText {
            get { return toolTipText; }
            set {
                if(string.IsNullOrEmpty(value)) {
                    toolTipText = DefaultToolTipText;
                } else if(value != toolTipText) {
                    toolTipText = value;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolTipText)));
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}