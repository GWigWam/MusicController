using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerInterface.Commands
{
    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute, Action<Exception>? exeptionHandler = null)
            : this(execute, DefaultCanExecute, exeptionHandler) { }

        public RelayCommand(Action execute, Func<bool> canExecute, Action<Exception>? exeptionHandler = null)
            : base(_ => execute(), _ => canExecute(), exeptionHandler) { }

        private static bool DefaultCanExecute() => true;
    }
}
