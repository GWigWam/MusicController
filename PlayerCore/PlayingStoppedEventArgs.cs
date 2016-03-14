using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

    public class PlayingStoppedEventArgs : EventArgs {

        public bool PlayedToEnd {
            get;
        }

        public Exception Exception {
            get;
        }

        public PlayingStoppedEventArgs(bool playedToEnd, Exception exception = null) {
            PlayedToEnd = playedToEnd;
            Exception = exception;
        }
    }
}