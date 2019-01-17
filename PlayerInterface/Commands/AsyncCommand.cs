using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.Commands {
    public class AsyncCommand : AsyncCommand<object> {
        public AsyncCommand(Func<Task> execute, Action<Task> continueWith = null) : base(_ => execute(), continueWith) {
        }

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute, Action<Task> continueWith = null) : base(_ => execute(), _ => canExecute(), continueWith) {
        }
    }
}
