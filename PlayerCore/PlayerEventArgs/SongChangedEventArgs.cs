using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.PlayerEventArgs {

    public class SongChangedEventArgs : EventArgs {
        public Song Previous { get; }
        public Song Next { get; }

        public SongChangedEventArgs(Song prev, Song next) {
            Previous = prev;
            Next = next;
        }
    }
}