using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerInterface.Commands
{
    public class RelayCommand<T> : BaseCommand
    {
        protected Action<T?> _Execute;
        protected Predicate<T?> _CanExecute;

        public Action<Exception>? ExceptionHandler;

        public RelayCommand(Action<T?> execute, Action<Exception>? exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) { }

        public RelayCommand(Action<T?> execute, Predicate<T?> canExecute, Action<Exception>? exeptionHandler = null)
        {
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            ExceptionHandler = exeptionHandler;
        }

        public override bool CanExecute(object parameter) => (parameter is T || parameter == null) && _CanExecute((T?)parameter);

        public override void Execute(object? parameter)
        {
            try
            {
                if (parameter is T parm)
                {
                    _Execute(parm);
                }
                else if (parameter is null)
                {
                    _Execute(default);
                }
                else
                {
                    throw new InvalidOperationException($"Expected parameter of type '{typeof(T).FullName}'.");
                }
            }
            catch (Exception e)
            {
                if (ExceptionHandler != null)
                {
                    ExceptionHandler(e);
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine($"Unhandled exception in {nameof(RelayCommand)}: {e}");
                }
            }
        }

        public void Execute(T parameter) => Execute(parameter as object);

        private static bool DefaultCanExecute(T? parameter) => true;
    }
}
