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

            SetupVolume(settings, player);

            SongStats.SetupStats(settings, player);

            var playlist = new Playlist();
            var transitionMngr = new TransitionManager(player, playlist, settings);
            transitionMngr.SongLoadErrorOccurred += (s, e) => ExceptionWindow.Show(e);

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
                        player.Play();
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

        private static void SetupVolume(AppSettings settings, SongPlayer player)
        {
            var volume = new PlayerVolume {
                MasterVolumeDb = settings.MasterVolumeDb,
                GainPreampDb = settings.GainPreampDb
            };
            setGain(player.CurrentSong);
            useVolume();

            settings.Changed += (s, a) => {
                if(a.ChangedPropertyName == nameof(AppSettings.MasterVolumeDb))
                {
                    volume.MasterVolumeDb = settings.MasterVolumeDb;
                }
                else if(a.ChangedPropertyName == nameof(AppSettings.GainPreampDb))
                {
                    volume.GainPreampDb = settings.GainPreampDb;
                }
                else if(a.ChangedPropertyName == nameof(AppSettings.UseFileGain))
                {
                    setGain(player.CurrentSong);
                }
            };

            player.SongChanged += (s, a) => {
                setGain(a.Next);
                useVolume();
            };

            volume.VolumeChanged += (_, _) => useVolume();

            void setGain(Song? song)
            {
                if(song is not null)
                {
                    volume.GainDb = settings.UseFileGain ? song.AlbumGain ?? song.TrackGain : null;
                }
            }

            void useVolume()
                => player.Volume = (float)volume.OutputVolume;
        }

        protected override void OnExit(ExitEventArgs e) {
            Exiting?.Invoke(this, e);
            base.OnExit(e);
        }
    }
}
