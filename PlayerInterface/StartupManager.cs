using Microsoft.VisualBasic.ApplicationServices;
using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using SpeechControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlayerInterface {

    public class StartupManager : WindowsFormsApplicationBase {
#if DEBUG
        public static bool IsDebug = true;
#else
        public static bool IsDebug = false;
#endif

        public static string AppSettingsFileName = "AppSettings.json";

        internal AppSettings ApplicationSettings {
            get; private set;
        }

        internal SongPlayer SongPlayer {
            get; private set;
        }

        internal Playlist Playlist {
            get; private set;
        }

        internal TransitionManager TransitionMgr {
            get; private set;
        }

        internal SpeechController SpeechController {
            get; private set;
        }

        protected static SpeechMusicControllerApp Application {
            get; private set;
        }

        [STAThread]
        public static void Main(string[] args) {
            var workingDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SpeechMusicController\\{(IsDebug ? "Debug\\" : "Release\\")}";
            if(!Directory.Exists(workingDir)) {
                Directory.CreateDirectory(workingDir);
            }
            Directory.SetCurrentDirectory(workingDir);

            new StartupManager().Run(args);
        }

        public StartupManager() {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs) {
            InitSettings();

            SongPlayer = new SongPlayer(ApplicationSettings.Volume);
            Playlist = new Playlist();
            TransitionMgr = new TransitionManager(SongPlayer, Playlist, ApplicationSettings);

            if(eventArgs.CommandLine.Count > 0) {
                ArgsPassed(eventArgs.CommandLine.ToArray());
            } else {
                LoadStartupSongFiles();
            }

            SpeechController = new SpeechController(SongPlayer, Playlist, ApplicationSettings);
            SpeechController.Init();

            Application = new SpeechMusicControllerApp();
            Application.InitializeComponent();

            var windowMgr = new WindowManager((Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)Application.FindResource("Tbi_Icon"));
            windowMgr.Init(ApplicationSettings, SongPlayer, Playlist, SpeechController);

            Application.Exiting += (s, a) => {
                ApplicationSettings.WriteToDisc();
            };

            windowMgr.Overlay.DisplayText("SMC Running...", 2000);
            Application.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs) {
            ArgsPassed(eventArgs.CommandLine.ToArray());
            base.OnStartupNextInstance(eventArgs);
        }

        protected void ArgsPassed(string[] args) {
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
                var added = Playlist.AddSong(songsToAdd);
                if(SongPlayer.PlayerState != PlayerState.Playing) {
                    Playlist.SelectFirstMatch(added.First());
                    SongPlayer.PlayerState = PlayerState.Playing;
                }
            }
        }

        private void InitSettings() {
            if(!File.Exists(AppSettingsFileName)) {
                var set = new AppSettings(AppSettingsFileName);
                set.WriteToDisc();
            }

            ApplicationSettings = SettingsFile.ReadSettingFile<AppSettings>(AppSettingsFileName);

            ApplicationSettings.Changed += (sender, args) => {
                if(args.ChangedPropertyName == nameof(AppSettings.Volume)) {
                    SongPlayer.Volume = ((AppSettings)sender).Volume;
                }
            };
        }

        private void LoadStartupSongFiles() {
            var startupSongFiles = new List<SongFile>();
            foreach(var path in ApplicationSettings.StartupFolders) {
                if(File.Exists(path)) {
                    startupSongFiles.Add(SongFileReader.ReadFile(path));
                } else if(Directory.Exists(path)) {
                    startupSongFiles.AddRange(SongFileReader.ReadFolderFiles(path));
                }
            }
            Playlist.AddSong(
                startupSongFiles
                .Where(sf => sf != null)
                .Select(sf => new Song(sf))
            );

            if(ApplicationSettings.ShuffleOnStartup) {
                Playlist.Shuffle();
            }
        }
    }
}