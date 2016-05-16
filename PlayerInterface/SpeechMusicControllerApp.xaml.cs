using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using SpeechControl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface {

    public partial class SpeechMusicControllerApp : Application {

        public event EventHandler<ExitEventArgs> Exiting;

        private void Application_Startup(object sender, StartupEventArgs e) {
        }

        protected override void OnExit(ExitEventArgs e) {
            Exiting?.Invoke(this, e);
            base.OnExit(e);
        }
    }
}