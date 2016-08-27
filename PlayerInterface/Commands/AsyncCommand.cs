using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.Commands {

    public class AsyncCommand : ICommand {
        private Func<object, Task> CommandExecute;
        private Predicate<object> CommandCanExecute;
        private Action<Task> ContinueWith;

        public bool IsExecuting { get; protected set; } = false;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => (!IsExecuting) && (CommandCanExecute?.Invoke(parameter) ?? false);

        public void Execute(object parameter) {
            IsExecuting = true;
            RaiseCanExecuteChanged();
            var task = CommandExecute(parameter);
            if(task.Status == TaskStatus.Created) {
                task.Start();
            }
            if(ContinueWith != null) {
                if(task.Status != TaskStatus.Canceled && task.Status != TaskStatus.Faulted && task.Status != TaskStatus.RanToCompletion) {
                    task.ContinueWith((t) => {
                        ContinueWith(t);
                        IsExecuting = false;
                        RaiseCanExecuteChanged();
                    });
                } else {
                    IsExecuting = false;
                    ContinueWith(task);
                }
            }
            RaiseCanExecuteChanged();
        }

        public AsyncCommand(Func<object, Task> execute, Action<Task> continueWith = null)
            : this(execute, DefaultCanExecute, continueWith) {
        }

        public AsyncCommand(Func<object, Task> execute, Predicate<object> canExecute, Action<Task> continueWith = null) {
            if(execute == null) {
                throw new ArgumentNullException("execute");
            }

            if(canExecute == null) {
                throw new ArgumentNullException("canExecute");
            }

            CommandExecute = execute;
            CommandCanExecute = canExecute;
            ContinueWith = continueWith;
        }

        public void RaiseCanExecuteChanged(EventArgs args = null) {
            Application.Current.Dispatcher.Invoke(() => {
                CanExecuteChanged?.Invoke(this, args ?? new EventArgs());
            });
        }

        private static bool DefaultCanExecute(object parameter) {
            return true;
        }
    }
}