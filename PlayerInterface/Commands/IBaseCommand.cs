using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.Commands {
    public interface IBaseCommand : ICommand {
        void RaiseCanExecuteChanged(EventArgs args = null);
    }

    public abstract class BaseCommand : IBaseCommand {
        public event EventHandler CanExecuteChanged;

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        public void RaiseCanExecuteChanged(EventArgs args = null) => Application.Current.Dispatcher.BeginInvoke((Action)(() => CanExecuteChanged?.Invoke(this, args ?? EventArgs.Empty)));

        public void BindCanExecuteToProperty(Action<PropertyChangedEventHandler> AttachHandler, string propertyName) {
            AttachHandler((s, a) => {
                if (a.PropertyName == propertyName) {
                    RaiseCanExecuteChanged();
                }
            });
        }
    }
}
