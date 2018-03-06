using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.Commands {

    public class RelayCommand : ICommand {
        public event EventHandler CanExecuteChanged;

        private Action<object> execute;

        private Predicate<object> canExecute;
        
        public RelayCommand(Action<object> execute, Action<Exception> exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute, Action<Exception> exeptionHandler = null) {
            this.execute = execute ?? throw new ArgumentNullException("execute");
            this.canExecute = canExecute ?? throw new ArgumentNullException("canExecute");
            ExceptionHandler = exeptionHandler;
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

        public void Destroy() {
            this.canExecute = _ => false;
            this.execute = _ => { return; };
        }

        private static bool DefaultCanExecute(object parameter) {
            return true;
        }
    }
}
