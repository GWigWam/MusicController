using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.Settings {

    public class SongStats : INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;

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

        private byte[]? _InfoHash;
        public byte[]? InfoHash {
            get => _InfoHash;
            set {
                if ((value == null && _InfoHash != null) || (value != null && _InfoHash == null) || (value != null && _InfoHash != null && !value.SequenceEqual(_InfoHash)))
                {
                    _InfoHash = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InfoHash)));
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
        
        public static byte[]? CalculateInfoHash(Song song)
        {
            if (song.Title != null && song.Artist != null)
            {
                var inpBytes = Encoding.UTF8.GetBytes($"{song.Artist}_{song.Album}_{song.Title}_{song.Track}_{song.Disc}");
                using var sha1 = System.Security.Cryptography.SHA1.Create();
                var hash = sha1.ComputeHash(inpBytes);
                return hash;
            }
            return null;
        }
    }
}
