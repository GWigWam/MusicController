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

        public IBaseCommand NextCommand {
            get; private set;
        }

        public IBaseCommand PreviousCommand {
            get; private set;
        }

        public NextPrevVm(SongPlayer player, Playlist playlist) {
            SongPlayer = player;
            Playlist = playlist;

            NextCommand = new RelayCommand(() => Playlist.Next(true), _ => Playlist.HasNext(true));
            Playlist.CurrentSongChanged += (s, a) => NextCommand.RaiseCanExecuteChanged();

            PreviousCommand = new RelayCommand(() => {
                if (SongPlayer.Elapsed.TotalMilliseconds > PreviousRestartMinTimeMs) {
                    SongPlayer.Elapsed = TimeSpan.FromMilliseconds(0);
                } else {
                    Playlist.Previous(true);
                }
            });
        }
    }
}
