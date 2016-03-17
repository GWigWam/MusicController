using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface {

    public abstract class NotifyPropertyChanged : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertiesChanged(params string[] propertyNames) {
            foreach(var propName in propertyNames) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}