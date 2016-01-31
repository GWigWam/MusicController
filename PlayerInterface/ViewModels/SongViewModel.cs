using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels {

    public class SongViewModel : INotifyPropertyChanged {
        public string Info => $"[{FormatHelper.FormatTimeSpan(Song?.File?.TrackLength)}] {Song?.File?.BitRate}kbps, {Song?.File?.Track}/{Song?.File?.TrackCount}, {Song?.File?.Year}";

        public string Path => $"{Song?.FilePath}";

        private bool playing = false;

        public bool Playing {
            get { return playing; }
            set {
                if(playing != value) {
                    playing = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Playing)));
                }
            }
        }

        public Song Song {
            get;
        }

        public string SubTitle => $"{Song?.Artist} ({Song?.Album})";
        public string Title => $"{Song?.Title}";

        public event PropertyChangedEventHandler PropertyChanged;

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
                ["Year"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.Year))
            };
        }

        public SongViewModel() {
        }

        public SongViewModel(Song song) {
            Song = song;
        }
    }
}