using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.Commands {

    public class GenericRelayCommand<T> : ICommand where T : class {
        private Action<T> execute;

        private Predicate<T> canExecute;

        private event EventHandler CanExecuteChangedInternal;

        public GenericRelayCommand(Action<T> execute, Action<Exception> exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) {
        }

        public GenericRelayCommand(Action<T> execute, Predicate<T> canExecute, Action<Exception> exeptionHandler = null) {
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
            return (parameter as T) != null && this.canExecute != null && this.canExecute((T)parameter);
        }

        public void Execute(object parameter) {
            try {
                execute((T)parameter);
            } catch(Exception e) {
                ExceptionHandler?.Invoke(e);
            }
        }

        public void Execute(T parameter) {
            Execute(parameter as object);
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

        private static bool DefaultCanExecute(T parameter) {
            return true;
        }
    }
}