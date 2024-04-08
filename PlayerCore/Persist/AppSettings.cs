using Newtonsoft.Json;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Persist
{
    public class AppSettings : PersistToFile
    {
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
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(StartMinimized)));
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
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(SongTransitionDelayMs)));
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
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(ScreenOverlayShowTimeMs)));
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
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(ResetSentenceTimeMs)));
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
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(WindowHeight)));
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
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(WindowWidth)));
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
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(Theme)));
                }
            }
        }

        private List<SongStats> statistics = new List<SongStats>();

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        protected List<SongStats> Statistics {
            get => statistics;
            set {
                statistics = value;
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
                if (_Queued == null || !_Queued.SequenceEqual(value))
                {
                    _Queued = value;
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(QueuedSongs)));
                }
            }
        }

        private int? _QueueIndx;
        [JsonProperty]
        public int? QueueIndex {
            get => _QueueIndx;
            set {
                if (_QueueIndx != value)
                {
                    _QueueIndx = value;
                    RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(QueueIndex)));
                }
            }
        }

        [JsonIgnore]
        public double _MasterVolumeDb = Volume.Linear.ToDecibel(0.5);
        [JsonProperty]
        public double MasterVolumeDb {
            get => _MasterVolumeDb;
            set {
                _MasterVolumeDb = value;
                RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(MasterVolumeDb)));
            }
        }

        [JsonIgnore]
        public bool _UseFileGain = false;
        [JsonProperty]
        public bool UseFileGain {
            get => _UseFileGain;
            set {
                _UseFileGain = value;
                RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(UseFileGain)));
            }
        }
        
        [JsonIgnore]
        public double _GainPreampDb = 0;
        [JsonProperty]
        public double GainPreampDb {
            get => _GainPreampDb;
            set {
                _GainPreampDb = value;
                RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(GainPreampDb)));
            }
        }

        [JsonIgnore]
        private string _LastfmUser = "";
        [JsonProperty]
        public string LastfmUser
        {
            get => _LastfmUser;
            set
            {
                _LastfmUser = value;
                RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(LastfmUser)));
            }
        }

        [JsonIgnore]
        private bool _LastfmAuthed = false;
        [JsonProperty]
        public bool LastfmAuthed
        {
            get => _LastfmAuthed;
            set
            {
                _LastfmAuthed = value;
                RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(LastfmAuthed)));
            }
        }

        [JsonIgnore]
        private List<string> _StartupSongs { get; set; } = new List<string>();

        [JsonIgnore]
        public IEnumerable<string> StartupSongPaths => _StartupSongs;

        public AppSettings(string filePath) : base(filePath) { }

        [JsonConstructor]
        protected AppSettings() : base() { }

        protected override async Task WriteToDiskInternalAsync() {
            await base.WriteToDiskInternalAsync();
            await WriteStartupSongsM3U();
        }

        protected override async Task AfterRead()
        {
            await base.AfterRead();
            CleanupDeadStats();
            await ReadStartupSongsM3U();
        }

        private async Task WriteStartupSongsM3U() {
            var writePath = $"{M3UFilePath}.writing";

            var m3u = new PlaylistFiles.M3U(StartupSongPaths);
            await m3u.WriteAsync(writePath, true);
            File.Delete(M3UFilePath);
            File.Move(writePath, M3UFilePath);
        }

        private async Task ReadStartupSongsM3U() {
            if(File.Exists(M3UFilePath)) {
                var m3u = await PlaylistFiles.M3U.ReadAsync(M3UFilePath);
                _StartupSongs = new List<string>(m3u.Paths.Distinct(StringComparer.OrdinalIgnoreCase));
            }
        }

#nullable enable
        public SongStats? GetSongStats(string songPath)
        {
            lock (Statistics)
            {
                return Statistics.FirstOrDefault(ss => ss.Path.Equals(songPath, StringComparison.OrdinalIgnoreCase));
            }
        }

        public bool TryFixSongStatsByTagsHash(string songPath, SongTags tags)
        {
            lock (Statistics)
            {
                var infHash = SongStats.CalculateInfoHash(tags);
                if (Statistics.FirstOrDefault(ss => ss.Path.Equals(songPath, StringComparison.OrdinalIgnoreCase)) is SongStats foundByPath)
                {
                    var oldHash = foundByPath.InfoHash;
                    foundByPath.InfoHash = infHash;
                    return (oldHash == null && infHash != null) || (oldHash != null && infHash == null) || (oldHash != null && infHash != null && !infHash.SequenceEqual(oldHash));
                }
                else if (infHash != null && Statistics.FirstOrDefault(ss => ss.InfoHash != null && ss.InfoHash.SequenceEqual(infHash) && !File.Exists(ss.Path)) is SongStats foundByHash) // File moved
                {
                    Statistics.Remove(foundByHash);
                    createSongStats(foundByHash.PlayCount, foundByHash.LastPlayed, infHash);
                    return true;
                }
                else
                {
                    createSongStats(0, null, infHash);
                    return true;
                }
            }

            SongStats createSongStats(int playCount, DateTimeOffset? lastPlayed, byte[]? infoHash)
            {
                var @new = new SongStats(songPath) {
                    PlayCount = playCount,
                    LastPlayed = lastPlayed,
                    InfoHash = infoHash
                };
                Statistics.Add(@new);
                @new.PropertyChanged += (s, a) => RaiseChanged(nameof(Statistics));
                RaiseChanged(nameof(Statistics));
                return @new;
            }
        }
#nullable restore

        public void CleanupDeadStats()
        {
            lock (Statistics)
            {
                var change = Statistics.Where(ss => ss.InfoHash != null || File.Exists(ss.Path)).ToList();
                if (change.Count != Statistics.Count)
                {
                    Statistics = change;
                }
            }
        }

        public bool IsStartupSong(Song song) => IsStartupSong(song.Path);
        public bool IsStartupSong(string path) => _StartupSongs.Any(ss => ss.Equals(path, StringComparison.OrdinalIgnoreCase));

        public void AddStartupSong(string songPath)
        {
            if(TryAddStartupSong(songPath))
            {
                RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(StartupSongPaths)));
            }
        }

        public void AddStartupSongs(IEnumerable<string> songPaths)
        {
            bool change = false;
            foreach(var path in songPaths)
            {
                change |= TryAddStartupSong(path);
            }
            if(change)
            {
                RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(StartupSongPaths)));
            }
        }

        public void RemoveStartupSong(string songPath)
        {
            if(TryRemoveStartupSong(songPath))
            {
                RaiseChanged(new PersistentPropertyChangedEventArgs(typeof(AppSettings), nameof(StartupSongPaths)));
            }
        }

        private bool TryAddStartupSong(string songPath)
        {
            if(!IsStartupSong(songPath))
            {
                _StartupSongs.Add(songPath);
                StartupSongsChanged?.Invoke(this, new StartupSongsChangedArgs(true, songPath));
                return true;
            }
            return false;
        }

        private bool TryRemoveStartupSong(string songPath)
        {
            if(_StartupSongs.FindIndex(ss => ss.Equals(songPath, StringComparison.OrdinalIgnoreCase)) is not -1 and var ix)
            {
                _StartupSongs.RemoveAt(ix);
                StartupSongsChanged?.Invoke(this, new StartupSongsChangedArgs(false, songPath));
                return true;
            }
            return false;
        }
    }
}
