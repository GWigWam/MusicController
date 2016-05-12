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
        public string Info => $"[{FormatHelper.FormatTimeSpan(Song?.File?.TrackLength)}] {Song?.File?.BitRate}kbps, {Song?.File?.Track}/{Song?.File?.TrackCount}, {Song?.File?.Year}";

        public string Path => $"{Song?.FilePath}";

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

        public string SubTitle => $"{Song?.Artist} ({Song?.Album})";
        public string Title => $"{Song?.Title}";

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
        }
    }
}