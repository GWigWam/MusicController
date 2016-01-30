using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels {

    public class SongViewModel {

        public Song Song {
            get;
        }

        public string Title => $"{Song?.Title}";
        public string SubTitle => $"{Song?.Artist} ({Song?.Album})";

        public string Info => $"[{FormatHelper.FormatTimeSpan(Song?.File?.TrackLength)}] {Song?.File?.BitRate}kbps, {Song?.File?.Track}/{Song?.File?.TrackCount}, {Song?.File?.Year}";

        public string Path => $"{Song?.FilePath}";

        public SongViewModel() {
        }

        public SongViewModel(Song song) {
            Song = song;
        }
    }
}