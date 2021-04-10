﻿using Newtonsoft.Json;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public class AppSettings : SettingsFile {
        private const string M3UFileName = "StartupSongs.m3u";

        public event StartupSongChangedHandler StartupSongsChanged;

        private bool startMinimized = true;
        
        private string M3UFilePath => !string.IsNullOrEmpty(FullFilePath) ? Path.Combine(new FileInfo(FullFilePath).DirectoryName, M3UFileName) : null;

        [JsonProperty]
        public bool StartMinimized {
            get { return startMinimized; }
            set {
                if(value != startMinimized) {
                    startMinimized = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartMinimized)));
                }
            }
        }

        private uint songTransitionDelayMs = 2000;

        [JsonProperty]
        public uint SongTransitionDelayMs {
            get { return songTransitionDelayMs; }
            set {
                if(value != songTransitionDelayMs) {
                    songTransitionDelayMs = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(SongTransitionDelayMs)));
                }
            }
        }

        private uint screenOverlayShowTimeMs = 1200;

        [JsonProperty]
        public uint ScreenOverlayShowTimeMs {
            get { return screenOverlayShowTimeMs; }
            set {
                if(value != screenOverlayShowTimeMs) {
                    screenOverlayShowTimeMs = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(ScreenOverlayShowTimeMs)));
                }
            }
        }

        private uint resetSentenceTimeMs = 6000;

        [JsonProperty]
        public uint ResetSentenceTimeMs {
            get { return resetSentenceTimeMs; }
            set {
                if(value != resetSentenceTimeMs) {
                    resetSentenceTimeMs = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(ResetSentenceTimeMs)));
                }
            }
        }

        private int windowHeight = 390;

        [JsonProperty]
        public int WindowHeight {
            get { return windowHeight; }
            set {
                if(value != windowHeight) {
                    windowHeight = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(WindowHeight)));
                }
            }
        }

        private int windowWidth = 300;

        [JsonProperty]
        public int WindowWidth {
            get { return windowWidth; }
            set {
                if(value != windowWidth) {
                    windowWidth = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(WindowWidth)));
                }
            }
        }

        private string theme = "Default";

        [JsonProperty]
        public string Theme {
            get { return theme; }
            set {
                if(value != theme) {
                    theme = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(Theme)));
                }
            }
        }

        private List<SongStats> statistics = new List<SongStats>();

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        protected List<SongStats> Statistics {
            get => statistics;
            set {
                statistics = value.Where(ss => File.Exists(ss.Path)).ToList();
                foreach(var stat in statistics) {
                    stat.PropertyChanged += (s, a) => RaiseChanged(nameof(Statistics));
                }
            }
        }

        private string[] _Queued;
        [JsonProperty]
        public string[] QueuedSongs {
            get => _Queued;
            set {
                _Queued = value;
                RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(QueuedSongs)));
            }
        }

        private int? _QueueIndx;
        [JsonProperty]
        public int? QueueIndex {
            get => _QueueIndx;
            set {
                _QueueIndx = value;
                RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(QueueIndex)));
            }
        }

        [JsonIgnore]
        private HashSet<Song> _StartupSongs { get; set; } = new HashSet<Song>();

        [JsonIgnore]
        public IEnumerable<Song> StartupSongs => _StartupSongs;

        public AppSettings(string filePath) : base(filePath) { }

        [JsonConstructor]
        protected AppSettings() : base() { }

        protected override async Task WriteToDiskInternalAsync() {
            await base.WriteToDiskInternalAsync();
            await WriteStartupSongsM3U();
        }

        protected override void AfterRead() {
            base.AfterRead();
            var readTask = Task.Run(ReadStartupSongsM3U);
            readTask.ConfigureAwait(false);
            readTask.Wait();
        }

        private async Task WriteStartupSongsM3U() {
            var writePath = $"{M3UFilePath}.writing";

            var m3u = new PlaylistFiles.M3U(StartupSongs);
            await m3u.WriteAsync(writePath, true);
            File.Delete(M3UFilePath);
            File.Move(writePath, M3UFilePath);
        }

        private async Task ReadStartupSongsM3U() {
            if(File.Exists(M3UFilePath)) {
                var m3u = await PlaylistFiles.M3U.ReadAsync(M3UFilePath);
                _StartupSongs = new HashSet<Song>(m3u.Files);
            }
        }

        public SongStats GetSongStats(Song song)
        {
            return Statistics.FirstOrDefault(ss => ss.Path.Equals(song.Path, StringComparison.OrdinalIgnoreCase)) ??
                createSongStats();

            SongStats createSongStats()
            {
                var @new = new SongStats(song.Path);
                Statistics.Add(@new);
                @new.PropertyChanged += (s, a) => RaiseChanged(nameof(Statistics));
                RaiseChanged(nameof(Statistics));
                return @new;
            }
        }

        public bool IsStartupSong(string path) => _StartupSongs.Any(sf => sf.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase));
        public bool IsStartupSong(Song song) => _StartupSongs.Contains(song);

        public void AddStartupSong(Song song) {
            if(TryAddStartupSong(song)) {
                RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartupSongs)));
            }
        }

        public void AddStartupSongs(IEnumerable<Song> songs) {
            bool change = false;
            foreach(var song in songs) {
                change |= TryAddStartupSong(song);
            }
            if(change) {
                RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartupSongs)));
            }
        }

        public void RemoveStartupSong(string path) => RemoveStartupSong(_StartupSongs.FirstOrDefault(sf => path.Equals(sf.Path, StringComparison.CurrentCultureIgnoreCase)));

        public void RemoveStartupSong(Song song) {
            if(TryRemoveStartupSong(song)) {
                RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartupSongs)));
            }
        }

        private bool TryAddStartupSong(Song song) {
            if(song != null && !IsStartupSong(song)) {
                _StartupSongs.Add(song);
                StartupSongsChanged?.Invoke(this, new StartupSongsChangedArgs(true, song));
                return true;
            }
            return false;
        }

        private bool TryRemoveStartupSong(Song song) {
            if(song != null) {
                if(_StartupSongs.Remove(song)) {
                    StartupSongsChanged?.Invoke(this, new StartupSongsChangedArgs(false, song));
                    return true;
                }
            }
            return false;
        }
    }
}
