using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Songs;
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

        internal Playlist SongList {
            get; private set;
        }

        internal TransitionManager TransitionMgr {
            get; private set;
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            SongPlayer = new SongPlayer(1/*Todo, get from settings*/);

            var songs = SongFileReader.ReadFolderFiles(@"F:\Zooi\OneDrive\Muziek\Green Day\", "*.mp3");
            SongList = new Playlist();
            SongList.AddSongs(songs.Select(sf => new Song(sf)));

            TransitionMgr = new TransitionManager(SongPlayer, SongList);
            SongPlayer.CurrentSong = SongList.CurrentSong;

            WindowMgr = new WindowManager(this);
            WindowMgr.Init(SongPlayer, SongList);
        }
    }
}