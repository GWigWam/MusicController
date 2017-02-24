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

        public SongStats(string path) {
            Path = path;
        }

        public static void SetupStats(AppSettings settings, SongPlayer player) {
            long playingSince = Environment.TickCount;

            player.SongChanged += (_, args) => {
                const int minPlayTimeMs = 5000;
                const float defVolume = 0.5f;

                if(args.Previous != null && Environment.TickCount - playingSince > minPlayTimeMs) {
                    var songStats = settings.SongStats.FirstOrDefault(ss => ss.Path == args.Previous.FilePath);
                    if(songStats == null) {
                        songStats = new SongStats(args.Previous.FilePath);
                        settings.AddSongStats(songStats);
                    }
                    if(songStats.Volume != player.Volume) {
                        songStats.Volume = player.Volume;
                    }
                }

                if(args.Next != null) {
                    var nextVolume = settings.SongStats.FirstOrDefault(ss => ss.Path == args.Next.FilePath)?.Volume;
                    if(nextVolume != null) {
                        player.Volume = nextVolume.Value;
                    } else {
                        player.Volume = defVolume;
                    }
                }

                playingSince = Environment.TickCount;
            };
        }
    }
}