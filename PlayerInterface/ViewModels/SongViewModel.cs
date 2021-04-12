using PlayerCore.Settings;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface.ViewModels {
    public class SongViewModel : NotifyPropertyChanged {

        public static readonly Dictionary<string, PropertyInfo> SortProperties;

        static SongViewModel() {
            var songtype = typeof(Song);
            var statType = typeof(SongStats);
            SortProperties = new Dictionary<string, PropertyInfo>() {
                //Song:
                ["Title"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Title)),
                ["Album"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Album)),
                ["Artist"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Artist)),
                ["Genre"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Genre)),
                ["Length"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.TrackLength)),
                ["Year"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Year)),
                ["Track #"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Track)),
                //Stats
                ["Play Count"] = statType.GetProperty(nameof(PlayerCore.Settings.SongStats.PlayCount))
            };
        }

        public Song Song { get; }

        private AppSettings Settings { get; }
        
        private bool playing = false;
        public bool Playing {
            get { return playing; }
            set {
                if(playing != value) {
                    playing = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Path => Song.Path;

        public string SubTitle => $"{Song.Artist} {(Song.Album is string a ? $"({a})" : null)}";
        public string Title => Song.Title;

        public string TrackLengthStr => $"[{FormatHelper.FormatTimeSpan(Song.TrackLength)}]";
        public string BitRateStr => $"{Song.BitRate}kbps";
        public string TrackNrStr =>
            Song.Track is int t ?
                Song.TrackCount is int tc ? $"{t}/{tc}" :
                $"#{t}" :
            string.Empty;

        public string YearStr => Song.Year is int y ? $"{y}" : string.Empty;

        private int _PlayCount;
        public int PlayCount {
            get => _PlayCount;
            set {
                if(value != _PlayCount)
                {
                    _PlayCount = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(PlayCountStr));
                }
            }
        }
        public string PlayCountStr => $"{PlayCount}x";

        private int? _QueueIndex = null;
        public int? QueueIndex {
            get => _QueueIndex;
            set {
                if (_QueueIndex != value) {
                    _QueueIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DisplayType curDisplay;
        public DisplayType CurDisplay {
            get { return curDisplay; }
            set {
                if(value != curDisplay) {
                    curDisplay = value;
                    RaisePropertyChanged(nameof(CurDisplay), nameof(FrontVisibility), nameof(MenuVisibility), nameof(DropUnderHintVisibility), nameof(MenuItems));
                }
            }
        }

        public Visibility FrontVisibility => CurDisplay == DisplayType.Front ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MenuVisibility => CurDisplay == DisplayType.Menu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DropUnderHintVisibility => CurDisplay == DisplayType.DropUnderHint ? Visibility.Visible : Visibility.Collapsed;

        public IEnumerable<SongMenuItemViewModel> MenuItems => GetMenuItems();

        private bool _IsSelected;
        public bool IsSelected {
            get => _IsSelected;
            set {
                if(_IsSelected != value) {
                    _IsSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                }
            }
        }

        public bool IsStartupSong => Settings.IsStartupSong(Song);

        private readonly Predicate<Song> isCurrentSong;
        private readonly Action<Song> playSong;
        private readonly Action<Song> enqueue;
        private readonly Action<Song> removeSong;

        /// <summary>
        /// For testing purposes only
        /// </summary>
        public SongViewModel() {
        }

        public SongViewModel(Song song, AppSettings settings, Predicate<Song> ics, Action<Song> ps, Action<Song> enqueue, Action<Song> rs) {
            curDisplay = DisplayType.Front;
            Song = song;
            Settings = settings;

            isCurrentSong = ics;
            playSong = ps;
            this.enqueue = enqueue;
            removeSong = rs;

            var stats = settings.GetSongStats(song);
            PlayCount = stats.PlayCount;
            stats.PropertyChanged += (s, a) => PlayCount = ((SongStats)s).PlayCount;
        }

        public static implicit operator Song(SongViewModel svm) => svm.Song;

        private IEnumerable<SongMenuItemViewModel> GetMenuItems() {
            if(CurDisplay != DisplayType.Menu) {
                yield break;
            }

            if(!isCurrentSong(Song)) {
                yield return new SongMenuItemViewModel("Play now", () => playSong(Song));
                yield return new SongMenuItemViewModel("Enqueue", () => enqueue(Song));
            }
            yield return new SongMenuItemViewModel("Remove", () => removeSong(Song));

            yield return new SongMenuItemViewModel("Open file location", OpenFileLocation);

            if(IsStartupSong) {
                yield return new SongMenuItemViewModel("Remove from startup songs", RemoveFromStartup);
            } else {
                yield return new SongMenuItemViewModel("Add to startup songs", AddToStartup);
            }
        }

        private void OpenFileLocation() {
            if(File.Exists(Path)) {
                System.Diagnostics.Process.Start("explorer.exe", $"/select, {Path}");
            }
        }
        
        private void AddToStartup() {
            Settings.AddStartupSong(Song.Path);
            RaisePropertyChanged(nameof(IsStartupSong));
        }

        private void RemoveFromStartup() {
            Settings.RemoveStartupSong(Song.Path);
            RaisePropertyChanged(nameof(IsStartupSong));
        }

        public enum DisplayType {
            Front, Menu, DropUnderHint
        }
    }
}
