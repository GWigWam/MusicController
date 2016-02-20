using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface {

    public class AsyncCommand : ICommand {
        private Action<object> CommandExecute;
        private Predicate<object> CommandCanExecute;
        private Action<Task> ContinueWith;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => CommandCanExecute?.Invoke(parameter) ?? false;

        public void Execute(object parameter) {
            var task = new Task(CommandExecute, parameter);
            if(ContinueWith != null) {
                task.ContinueWith((t) => {
                    ContinueWith(t);
                    RaiseCanExecuteChanged();
                });
            }
            task.Start();
            RaiseCanExecuteChanged();
        }

        public AsyncCommand(Action<object> execute, Action<Task> continueWith = null)
            : this(execute, DefaultCanExecute, continueWith) {
        }

        public AsyncCommand(Action<object> execute, Predicate<object> canExecute, Action<Task> continueWith = null) {
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