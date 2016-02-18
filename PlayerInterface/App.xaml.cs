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

    public partial class App : Application {

        internal WindowManager WindowMgr {
            get; private set;
        }

        private AppSettings Settings {
            get; set;
        }

        private SongPlayer SongPlayer {
            get; set;
        }

        private Playlist Playlist {
            get; set;
        }

        private SpeechController SpeechControl {
            get; set;
        }

        /// <summary>
        /// DO NOT USE
        /// </summary>
        private App() {
            throw new InvalidOperationException("This ctor should not be used");
        }

        public App(AppSettings settings, SongPlayer songPlayer, Playlist playlist, SpeechController speechControl) {
            Settings = settings;
            SongPlayer = songPlayer;
            Playlist = playlist;
            SpeechControl = speechControl;
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            WindowMgr = new WindowManager((TaskbarIcon)FindResource("Tbi_Icon"));
            WindowMgr.Init(Settings, SongPlayer, Playlist, SpeechControl);
        }

        protected override void OnExit(ExitEventArgs e) {
            Settings.WriteToDisc(false);
            base.OnExit(e);
        }
    }
}