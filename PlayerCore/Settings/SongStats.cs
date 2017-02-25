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
            long playingSince = Environment.TickCount;

            player.SongChanged += (_, args) => {
                const int minPlayTimeMs = 5000;
                const float defVolume = 0.5f;

                if(args.Previous != null && Environment.TickCount - playingSince > minPlayTimeMs) {
                    var songStats = args.Previous.Stats;
                    if(songStats.Volume != player.Volume) {
                        songStats.Volume = player.Volume;
                    }
                }

                if(args.Next != null) {
                    var nextVolume = args.Next.Stats.Volume;
                    if(nextVolume.HasValue) {
                        player.Volume = nextVolume.Value;
                    } else {
                        player.Volume = defVolume;
                    }
                }

                playingSince = Environment.TickCount;
            };

            player.PlayingStopped += (_, a) => {
                if(a.PlayedToEnd && player.CurrentSong != null) {
                    player.CurrentSong.Stats.PlayCount++;
                }
            };
        }
    }
}