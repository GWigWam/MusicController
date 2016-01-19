using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface {

    public partial class App : Application {

        internal WindowManager WindowMgr {
            get; private set;
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            WindowMgr = new WindowManager(this);
            WindowMgr.Init();
        }
    }
}