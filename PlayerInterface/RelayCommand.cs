using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface {

    public class RelayCommand : ICommand {
        private Action<object> execute;

        private Predicate<object> canExecute;

        private event EventHandler CanExecuteChangedInternal;

        public RelayCommand(Action<object> execute, Action<Exception> exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute, Action<Exception> exeptionHandler = null) {
            if(execute == null) {
                throw new ArgumentNullException("execute");
            }

            if(canExecute == null) {
                throw new ArgumentNullException("canExecute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
            ExceptionHandler = exeptionHandler;
        }

        public event EventHandler CanExecuteChanged {
            add {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        public Action<Exception> ExceptionHandler;

        public bool CanExecute(object parameter) {
            return this.canExecute != null && this.canExecute(parameter);
        }

        public void Execute(object parameter) {
            try {
                execute(parameter);
            } catch(Exception e) {
                ExceptionHandler?.Invoke(e);
            }
        }

        public void OnCanExecuteChanged() {
            EventHandler handler = this.CanExecuteChangedInternal;
            if(handler != null) {
                //DispatcherHelper.BeginInvokeOnUIThread(() => handler.Invoke(this, EventArgs.Empty));
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Destroy() {
            this.canExecute = _ => false;
            this.execute = _ => { return; };
        }

        private static bool DefaultCanExecute(object parameter) {
            return true;
        }
    }
}