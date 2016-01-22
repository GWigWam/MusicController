using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

    public class PlaybackStateChangedEventArgs : EventArgs {
        public PlaybackState? OldState { get; }
        public PlaybackState NewState { get; }

        public PlaybackStateChangedEventArgs(PlaybackState? oldState, PlaybackState newState) {
            OldState = oldState;
            NewState = newState;
        }
    }
}