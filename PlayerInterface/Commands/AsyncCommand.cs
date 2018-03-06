using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.Commands {

    public class AsyncCommand : BaseCommand {

        private Func<object, Task> CommandExecute;
        private Predicate<object> CommandCanExecute;
        private Action<Task> ContinueWith;

        public bool IsExecuting { get; protected set; } = false;

        public AsyncCommand(Func<object, Task> execute, Action<Task> continueWith = null)
            : this(execute, DefaultCanExecute, continueWith) {
        }

        public AsyncCommand(Func<object, Task> execute, Predicate<object> canExecute, Action<Task> continueWith = null) {
            CommandExecute = execute ?? throw new ArgumentNullException("execute");
            CommandCanExecute = canExecute ?? throw new ArgumentNullException("canExecute");
            ContinueWith = continueWith ?? DefaultContinueWith;
        }

        public override bool CanExecute(object parameter) => (!IsExecuting) && (CommandCanExecute?.Invoke(parameter) ?? false);

        public override void Execute(object parameter) {
            IsExecuting = true;
            RaiseCanExecuteChanged();
            var task = CommandExecute(parameter);
            task.ContinueWith((t) => {
                try {
                    ContinueWith(t);
                } finally {
                    IsExecuting = false;
                    RaiseCanExecuteChanged();
                }
            });
            RaiseCanExecuteChanged();
        }

        private static bool DefaultCanExecute(object parameter) => true;

        private static void DefaultContinueWith(Task t) {
            if (t.IsFaulted) {
                throw t.Exception;
            }
        }
    }
}