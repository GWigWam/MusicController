using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public class SongStats : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public string Path { get; }

        private int playCount = 0;
        public int PlayCount {
            get { return playCount; }
            set {
                if(value != playCount) {
                    playCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayCount)));
                }
            }
        }

        public SongStats(string path) {
            Path = path;
        }

        public static void SetupStats(AppSettings settings, SongPlayer player)
        {
            player.PlayingStopped += (_, a) => {
                if(a.PlayedToEnd && player.CurrentSong != null)
                {
                    settings.GetSongStats(player.CurrentSong).PlayCount++;
                }
            };
        }
    }
}
