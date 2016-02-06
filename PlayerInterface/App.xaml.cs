using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
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
        private const string AppSettingsPath = "AppSettings.json";

        internal AppSettings ApplicationSettings {
            get; private set;
        }

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
            InitSettings();

            SongPlayer = new SongPlayer(ApplicationSettings.Volume);
            ApplicationSettings.Changed += ApplicationSettings_Changed;

            //var songs = SongFileReader.ReadFolderFiles(@"F:\Zooi\OneDrive\Muziek\Green Day\", "*.mp3");
            SongList = new Playlist();
            //SongList.AddSongs(songs.Select(sf => new Song(sf)));

            TransitionMgr = new TransitionManager(SongPlayer, SongList, ApplicationSettings);
            //SongPlayer.CurrentSong = SongList.CurrentSong;

            WindowMgr = new WindowManager(this);
            WindowMgr.Init();
        }

        private void InitSettings() {
            if(!File.Exists(AppSettingsPath)) {
                var set = new AppSettings(AppSettingsPath);
                set.WriteToDisc(false);
            }

            ApplicationSettings = SettingsFile.ReadSettingFile<AppSettings>(AppSettingsPath);
            Exit += (s, a) => {
                ApplicationSettings.WriteToDisc(false);
            };
        }

        private void ApplicationSettings_Changed(object sender, SettingChangedEventArgs e) {
            if(e.ChangedPropertyName == nameof(AppSettings.Volume)) {
                SongPlayer.Volume = ((AppSettings)sender).Volume;
            }
        }
    }
}