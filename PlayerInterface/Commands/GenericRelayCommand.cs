using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.Commands {
    public class GenericRelayCommand<T> : ICommand {

        private Action<T> execute;
        private Predicate<T> canExecute;

        private event EventHandler CanExecuteChangedInternal;
        public event EventHandler CanExecuteChanged {
            add {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }

            remove {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        public Action<Exception> ExceptionHandler;

        public bool CanExecute(object parameter) => parameter is T tParam && canExecute(tParam);

        public GenericRelayCommand(Action<T> execute, Action<Exception> exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) { }

        public GenericRelayCommand(Action<T> execute, Predicate<T> canExecute, Action<Exception> exeptionHandler = null) {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            ExceptionHandler = exeptionHandler;
        }

        public void Execute(object parameter) {
            try {
                execute((T)parameter);
            } catch(Exception e) {
                ExceptionHandler?.Invoke(e);
            }
        }

        public void Execute(T parameter) => Execute(parameter as object);

        public void OnCanExecuteChanged() => CanExecuteChangedInternal?.Invoke(this, EventArgs.Empty);

        private static bool DefaultCanExecute(T parameter) => true;
    }
}