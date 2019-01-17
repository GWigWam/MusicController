using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.Commands {
    public class RelayCommand<T> : BaseCommand {

        private Action<T> execute;
        private Predicate<T> canExecute;
        
        public Action<Exception> ExceptionHandler;

        public RelayCommand(Action<T> execute, Action<Exception> exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) { }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute, Action<Exception> exeptionHandler = null) {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            ExceptionHandler = exeptionHandler;
        }

        public override bool CanExecute(object parameter) => (parameter is T || parameter == null) && canExecute((T)parameter);

        public override void Execute(object parameter) {
            try {
                if (parameter is T parm) {
                    execute(parm);
                } else {
                    throw new InvalidOperationException($"Expected parameter of type '{typeof(T).FullName}'.");
                }
            } catch(Exception e) {
                ExceptionHandler?.Invoke(e);
            }
        }

        public void Execute(T parameter) => Execute(parameter as object);
        
        private static bool DefaultCanExecute(T parameter) => true;
    }
}
