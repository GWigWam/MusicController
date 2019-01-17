using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.Commands {

    public class RelayCommand : BaseCommand {

        private readonly Action execute;
        private readonly Func<bool> canExecute;
        private readonly Action<Exception> ExceptionHandler;

        public RelayCommand(Action execute, Action<Exception> exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) {
        }

        public RelayCommand(Action execute, Func<bool> canExecute, Action<Exception> exeptionHandler = null) {
            this.execute = execute ?? throw new ArgumentNullException("execute");
            this.canExecute = canExecute ?? throw new ArgumentNullException("canExecute");
            ExceptionHandler = exeptionHandler ?? DefaultExceptionHandler;
        }

        public override bool CanExecute(object parameter) => canExecute();

        public override void Execute(object parameter) {
            try {
                execute();
            } catch(Exception e) {
                ExceptionHandler(e);
            }
        }

        private static bool DefaultCanExecute() => true;

        private static void DefaultExceptionHandler(Exception e) => throw e;
    }
}
