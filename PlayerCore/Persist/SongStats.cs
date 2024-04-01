using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.Persist {

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

        private DateTimeOffset? _LastPlayed;
        public DateTimeOffset? LastPlayed {
            get => _LastPlayed;
            set {
                if (value != _LastPlayed)
                {
                    _LastPlayed = value;
                    PropertyChanged?.Invoke(this, new(nameof(LastPlayed)));
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
                    settings.GetSongStats(player.CurrentSong).LastPlayed = DateTimeOffset.Now;
                }
            };
        }
        
        public static byte[]? CalculateInfoHash(Song song)
        {
            if (song.Title != null && song.Tags?.Artist is string artist)
            {
                var inpBytes = Encoding.UTF8.GetBytes($"{artist}_{song.Tags?.Album}_{song.Title}".ToLower());
                using var sha1 = System.Security.Cryptography.SHA1.Create();
                var hash = sha1.ComputeHash(inpBytes);
                return hash;
            }
            return null;
        }
    }
}
