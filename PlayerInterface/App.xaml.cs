using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Themes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface {

    public partial class MusicControllerApp : Application {
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

            ThemeManager.Instance.SetTheme(settings.Theme);
            settings.Changed += (s, a) => {
                if(a.ChangedPropertyName == nameof(AppSettings.Theme)) {
                    ThemeManager.Instance.SetTheme(settings.Theme);
                }
            };

            var windowMgr = new WindowManager((TaskbarIcon)FindResource(TrayIconResourceName));
            windowMgr.Init(settings, player, playlist, transitionMngr);

            LoadSongsBackground(e.Args, playlist, player, settings);

            Exiting += (s, a) => {
                PersistentQueue.SaveQueue(playlist, settings);
                settings.WriteToDisc();
            };

            windowMgr.Overlay.DisplayText("SMC Running...", 2000);
        }

        private static void LoadSongsBackground(string[] appArgs, Playlist playlist, SongPlayer player, AppSettings settings) => Task.Run(async () =>
        {
            var loadSpecific = appArgs.Length > 0;
            var paths = loadSpecific ? appArgs : getStartupSongsShuffled();
            if(paths.Any())
            {
                var fst = await Song.CreateAsync(paths.First());
                if(fst != null)
                {
                    var added = playlist.AddSong(fst);
                    playlist.SelectFirstMatch(added);
                    if(loadSpecific)
                    {
                        player.PlayerState = PlayerState.Playing;
                    }
                }
                await playlist.AddSongsAsync(SongPathsHelper.CreateSongs(paths.Skip(1)));
                await PersistentQueue.RestoreQueue(playlist, settings);
            }

            string[] getStartupSongsShuffled()
            {
                var r = new Random();
                return settings.StartupSongPaths.OrderBy(_ => r.NextDouble()).ToArray();
            }
        });

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
