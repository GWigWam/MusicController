using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.Commands {

    public class AsyncCommand<T> : BaseCommand {

        private Func<T, Task> CommandExecute;
        private Predicate<T> CommandCanExecute;
        private Action<Task> ContinueWith;

        public bool IsExecuting { get; protected set; } = false;

        public AsyncCommand(Func<T, Task> execute, Action<Task> continueWith = null)
            : this(execute, DefaultCanExecute, continueWith) {
        }

        public AsyncCommand(Func<T, Task> execute, Predicate<T> canExecute, Action<Task> continueWith = null) {
            CommandExecute = execute ?? throw new ArgumentNullException("execute");
            CommandCanExecute = canExecute ?? throw new ArgumentNullException("canExecute");
            ContinueWith = continueWith ?? DefaultContinueWith;
        }

        public override bool CanExecute(object parameter) {
            if(parameter is T || parameter == null) {
                return (!IsExecuting) && (CommandCanExecute?.Invoke((T)parameter) ?? false);
            } else {
                throw WrongTypeError(parameter);
            }
        }

        public override void Execute(object parameter) {
            IsExecuting = true;
            RaiseCanExecuteChanged();
            if(parameter is T || parameter == null) {
                var task = CommandExecute((T)parameter);
                task.ContinueWith((t) => {
                    try {
                        ContinueWith(t);
                    } finally {
                        IsExecuting = false;
                        RaiseCanExecuteChanged();
                    }
                });
                RaiseCanExecuteChanged();
            } else {
                throw WrongTypeError(parameter);
            }
        }

        private Exception WrongTypeError(object param) => new InvalidCastException($"Cannot use '{param}' of type {param.GetType().Name}, for command of type T: '{typeof(T).Name}'.");

        private static bool DefaultCanExecute(T parameter) => true;

        private static void DefaultContinueWith(Task t) {
            if (t.IsFaulted) {
                throw t.Exception;
            }
        }
    }
}
