using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface {

    public class StartupManager : WindowsFormsApplicationBase {

        protected static App Application {
            get; private set;
        }

        [STAThread]
        public static void Main(string[] args) {
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            new StartupManager().Run(args);
        }

        public StartupManager() {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs) {
            Application = new App();
            Application.InitializeComponent();

            Application.Startup += (s, a) => {
                Application.ArgsPassed(eventArgs.CommandLine.ToArray());
            };

            Application.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs) {
            Application.ArgsPassed(eventArgs.CommandLine.ToArray());
            base.OnStartupNextInstance(eventArgs);
        }
    }
}