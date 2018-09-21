using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface {

    public abstract class NotifyPropertyChanged : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(params string[] propertyNames) {
            foreach(var propName in propertyNames) {
                RaisePropertyChanged(propName);
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            if(propertyName != null) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
