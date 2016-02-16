using Microsoft.VisualBasic.ApplicationServices;
using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using SpeechControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface {

    public class StartupManager : WindowsFormsApplicationBase {
        public const string AppSettingsPath = "AppSettings.json";

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

        protected static App Application {
            get; private set;
        }

        [STAThread]
        public static void Main(string[] args) {
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
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

            Application = new App(ApplicationSettings, SongPlayer, Playlist);
            Application.InitializeComponent();
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
                Playlist.AddSongs(songsToAdd);
                Playlist.PlayFirstMatch(songsToAdd.First());
                SongPlayer.PlayerState = NAudio.Wave.PlaybackState.Playing;
            }
        }

        private void InitSettings() {
            if(!File.Exists(AppSettingsPath)) {
                var set = new AppSettings(AppSettingsPath);
                set.WriteToDisc(false);
            }

            ApplicationSettings = SettingsFile.ReadSettingFile<AppSettings>(AppSettingsPath);

            ApplicationSettings.Changed += ApplicationSettings_Changed;

            Shutdown += (s, a) => {
                ApplicationSettings.WriteToDisc(false);
            };
        }

        private void ApplicationSettings_Changed(object sender, SettingChangedEventArgs e) {
            if(e.ChangedPropertyName == nameof(AppSettings.Volume)) {
                SongPlayer.Volume = ((AppSettings)sender).Volume;
            }
        }

        private void LoadStartupSongFiles() {
            var startupSongFiles = new List<SongFile>();
            foreach(var path in ApplicationSettings.StartupFolders) {
                if(File.Exists(path)) {
                    startupSongFiles.Add(SongFileReader.ReadFile(path));
                } else if(Directory.Exists(path)) {
                    startupSongFiles.AddRange(SongFileReader.ReadFolderFiles(path, "*.mp3"));
                }
            }
            Playlist.AddSongs(startupSongFiles.Where(sf => sf != null).Select(sf => new Song(sf)));

            if(ApplicationSettings.ShuffleOnStartup) {
                Playlist.Shuffle();
            }
        }
    }
}