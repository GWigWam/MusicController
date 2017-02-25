using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface.ViewModels {

    public class SongViewModel : NotifyPropertyChanged {
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

        public Song Song {
            get;
        }

        public string Path => $"{Song.FilePath}";

        public string SubTitle => $"{Song.Artist} {(string.IsNullOrEmpty(Song.Album) ? string.Empty : $"({Song.Album})")}";
        public string Title => $"{Song.Title}";

        public string TrackLengthStr => $"[{FormatHelper.FormatTimeSpan(Song.File.TrackLength)}]";
        public string BitRateStr => $"{Song.File.BitRate}kbps";
        public string TrackNrStr => Song.File.TrackCount == 0 ? $"#{Song.File.Track}" : $"{Song.File.Track}/{Song.File.TrackCount}";
        public string YearStr => Song.File.Year != 0 ? $"{Song.File.Year}" : "-";
        public string PlayCountStr => $"{Song.Stats.PlayCount}x";

        public enum DisplayType {
            Front, Menu, DropUnderHint
        }

        private DisplayType curDisplay;

        public DisplayType CurDisplay {
            get { return curDisplay; }
            set {
                if(value != curDisplay) {
                    curDisplay = value;
                    RaisePropertiesChanged(nameof(CurDisplay), nameof(FrontVisibility), nameof(MenuVisibility), nameof(DropUnderHintVisibility));
                }
            }
        }

        public Visibility FrontVisibility => CurDisplay == DisplayType.Front ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MenuVisibility => CurDisplay == DisplayType.Menu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DropUnderHintVisibility => CurDisplay == DisplayType.DropUnderHint ? Visibility.Visible : Visibility.Collapsed;

        public FullPlayerViewModel MainViewModel {
            get;
        }

        public static Dictionary<string, PropertyInfo> SortProperties;

        static SongViewModel() {
            var songtype = typeof(Song);
            var filetype = typeof(SongFile);
            SortProperties = new Dictionary<string, PropertyInfo>() {
                //Song:
                ["Title"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Title)),
                ["Album"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Album)),
                ["Artist"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Artist)),
                //SongFile:
                ["Genre"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.Genre)),
                ["Length"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.TrackLength)),
                ["Year"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.Year)),
                ["Track #"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.Track))
            };
        }

        /// <summary>
        /// For testing purposes only
        /// </summary>
        public SongViewModel() {
        }

        public SongViewModel(Song song, FullPlayerViewModel fpvm) {
            curDisplay = DisplayType.Front;
            Song = song;
            MainViewModel = fpvm;

            Song.Stats.PropertyChanged += (s, a) => RaisePropertiesChanged(nameof(PlayCountStr));
        }
    }
}