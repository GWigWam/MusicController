using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Themes;
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
#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif
        private const string TrayIconResourceName = "Tbi_Icon";

        public static string AppSettingsFileName = "AppSettings.json";

        public event EventHandler<ExitEventArgs> Exiting;

        private void Application_Startup(object sender, StartupEventArgs e) {
            var workingDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SpeechMusicController\\{(IsDebug ? "Debug\\" : "Release\\")}";
            if(!Directory.Exists(workingDir)) {
                Directory.CreateDirectory(workingDir);
            }
            Directory.SetCurrentDirectory(workingDir);

            var settings = InitSettings();
            new AutoSave(settings, 60 * 10);

            var player = new SongPlayer();
            player.PlayingStopped += (s, a) => {
                if(a.Exception != null) {
                    Current.Dispatcher.Invoke(() => {
                        new ExceptionWindow(a.Exception).Show();
                    });
                }
            };

            SongStats.SetupStats(settings, player);

            var playlist = new Playlist();
            var transitionMngr = new TransitionManager(player, playlist, settings);

            if(e.Args.Length > 0) {
                var songs = SongPathsHelper.CreateSongs(settings, e.Args).ToArray();
                if(songs.Length > 0) {
                    var added = playlist.AddSong(songs);
                    if(player.PlayerState != PlayerState.Playing) {
                        playlist.SelectFirstMatch(added.First());
                        player.PlayerState = PlayerState.Playing;
                    }
                }
            } else {
                var startupSongFiles = settings.StartupSongs.Select(sf => new Song(sf, settings));
                playlist.AddSong(startupSongFiles);
                playlist.Shuffle();
            }

            PersistentQueue.RestoreQueue(playlist, settings);

            var speechController = new SpeechController(player, playlist, settings);
            speechController.Init();

            ThemeManager.Instance.SetTheme(settings.Theme);
            settings.Changed += (s, a) => {
                if(a.ChangedPropertyName == nameof(AppSettings.Theme)) {
                    ThemeManager.Instance.SetTheme(settings.Theme);
                }
            };

            var windowMgr = new WindowManager((TaskbarIcon)FindResource(TrayIconResourceName));
            windowMgr.Init(settings, player, playlist, speechController, transitionMngr);

            Exiting += (s, a) => {
                PersistentQueue.SaveQueue(playlist, settings);
                settings.WriteToDisc();
            };

            windowMgr.Overlay.DisplayText("SMC Running...", 2000);
        }

        private AppSettings InitSettings() {
            if(!File.Exists(AppSettingsFileName)) {
                var set = new AppSettings(AppSettingsFileName);
                set.WriteToDisc();
            }
            return SettingsFile.ReadSettingFile<AppSettings>(AppSettingsFileName);
        }

        protected override void OnExit(ExitEventArgs e) {
            Exiting?.Invoke(this, e);
            base.OnExit(e);
        }
    }
}
