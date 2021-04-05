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

        private float? volume = null;
        public float? Volume {
            get { return volume; }
            set {
                if(value != volume) {
                    volume = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                }
            }
        }

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

        public static void SetupStats(AppSettings settings, SongPlayer player) {
            player.SongChanged += (_, args) => {
                const float defVolume = 0.5f;

                if(args.Previous is Song prev)
                {
                    var prevStats = settings.GetSongStats(prev.File);
                    prevStats.Volume = player.Volume;
                }

                if(args.Next is Song next)
                {
                    var nextStats = settings.GetSongStats(next.File);
                    var nextVolume = nextStats.Volume;

                    if(nextVolume.HasValue) {
                        player.Volume = nextVolume.Value;
                    } else {
                        player.Volume = defVolume;
                    }
                }
            };

            player.PlayingStopped += (_, a) => {
                if(a.PlayedToEnd && player.CurrentSong != null) {
                    settings.GetSongStats(player.CurrentSong.File).PlayCount++;
                }
            };
        }
    }
}
