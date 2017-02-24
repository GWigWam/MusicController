using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.PlayerEventArgs {

    public class PlaybackStateChangedEventArgs : EventArgs {
        public PlayerState? OldState { get; }
        public PlayerState NewState { get; }

        public PlaybackStateChangedEventArgs(PlayerState? oldState, PlayerState newState) {
            OldState = oldState;
            NewState = newState;
        }
    }
}