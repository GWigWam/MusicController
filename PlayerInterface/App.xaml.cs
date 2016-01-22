using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface {

    public partial class App : Application {

        internal SongPlayer SongPlayer {
            get; private set;
        }

        internal WindowManager WindowMgr {
            get; private set;
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            SongPlayer = new SongPlayer(1/*Todo, get from settings*/);
            SongPlayer.CurrentSong = new Song("TestTitle", "TestArtist", "TestAlbum", "testsong.mp3");

            WindowMgr = new WindowManager(this);
            WindowMgr.Init(SongPlayer);
        }
    }
}