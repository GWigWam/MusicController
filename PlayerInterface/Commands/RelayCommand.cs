using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.Commands {

    public class RelayCommand : BaseCommand {

        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;
        private readonly Action<Exception> ExceptionHandler;

        public RelayCommand(Action<object> execute, Action<Exception> exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute, Action<Exception> exeptionHandler = null) {
            this.execute = execute ?? throw new ArgumentNullException("execute");
            this.canExecute = canExecute ?? throw new ArgumentNullException("canExecute");
            ExceptionHandler = exeptionHandler ?? DefaultExceptionHandler;
        }

        public override bool CanExecute(object parameter) => canExecute(parameter);

        public override void Execute(object parameter) {
            try {
                execute(parameter);
            } catch(Exception e) {
                ExceptionHandler(e);
            }
        }

        private static bool DefaultCanExecute(object parameter) => true;

        private static void DefaultExceptionHandler(Exception e) => throw e;
    }
}
