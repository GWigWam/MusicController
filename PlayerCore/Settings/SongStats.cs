using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public class SongStats {
        public string Path { get; }

        public float? Volume { get; set; }

        public SongStats(string path) {
            Path = path;
        }

        public static void SetupStats(AppSettings settings, SongPlayer player) {
            long playingSince = Environment.TickCount;

            player.SongChanged += (_, args) => {
                const int minPlayTimeMs = 5000;
                const float defVolume = 0.5f;

                if(args.Previous != null && Environment.TickCount - playingSince > minPlayTimeMs) {
                    var songStats = settings.Statistics.FirstOrDefault(ss => ss.Path == args.Previous.FilePath);
                    if(songStats == null) {
                        songStats = new SongStats(args.Previous.FilePath);
                        settings.Statistics.Add(songStats);
                    }
                    if(songStats.Volume != player.Volume) {
                        songStats.Volume = player.Volume;
                        settings.RaiseChanged(nameof(AppSettings.Statistics));
                    }
                }

                if(args.Next != null) {
                    var nextVolume = settings.Statistics.FirstOrDefault(ss => ss.Path == args.Next.FilePath)?.Volume;
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