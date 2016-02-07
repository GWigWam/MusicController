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

        public void ArgsPassed(string[] args) {
            var songfiles = new List<SongFile>();
            foreach(var arg in args) {
                if(File.Exists(arg)) {
                    var read = SongFileReader.ReadFile(arg);
                    if(read != null) {
                        songfiles.Add(read);
                    }
                }
            }

            if(songfiles.Count > 0) {
                var songsToAdd = songfiles.Select(sf => new Song(sf));
                SongList.AddSongs(songsToAdd);
                SongList.PlayFirstMatch(songsToAdd.First());
                SongPlayer.PlayerState = NAudio.Wave.PlaybackState.Playing;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            InitSettings();

            SongPlayer = new SongPlayer(ApplicationSettings.Volume);
            SongList = new Playlist();

            TransitionMgr = new TransitionManager(SongPlayer, SongList, ApplicationSettings);

            WindowMgr = new WindowManager(this);
            WindowMgr.Init();
        }

        private void InitSettings() {
            if(!File.Exists(AppSettingsPath)) {
                var set = new AppSettings(AppSettingsPath);
                set.WriteToDisc(false);
            }

            ApplicationSettings = SettingsFile.ReadSettingFile<AppSettings>(AppSettingsPath);

            ApplicationSettings.Changed += ApplicationSettings_Changed;
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