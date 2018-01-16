using PlayerCore;
using PlayerInterface.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {
    public class NextPrevVm {
        private const int PreviousRestartMinTimeMs = 5000;

        public SongPlayer SongPlayer { get; }

        public Playlist Playlist { get; }

        public ICommand NextCommand {
            get; private set;
        }

        public ICommand PreviousCommand {
            get; private set;
        }

        public NextPrevVm(SongPlayer player, Playlist playlist) {
            SongPlayer = player;
            Playlist = playlist;

            NextCommand = new RelayCommand((o) => {
                Playlist.CurrentSongIndex++;
            }, (o) => Playlist.HasNext);

            PreviousCommand = new RelayCommand((o) => {
                if (SongPlayer.Elapsed.TotalMilliseconds > PreviousRestartMinTimeMs) {
                    SongPlayer.Elapsed = TimeSpan.FromMilliseconds(0);
                } else {
                    Playlist.CurrentSongIndex--;
                }
            }, (o) => Playlist.HasPrevious || SongPlayer.Elapsed.TotalMilliseconds > PreviousRestartMinTimeMs);
        }
    }
}
