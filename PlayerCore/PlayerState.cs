using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

    /// <summary>
    /// Mirror of NAudio.Wave.PlaybackState
    /// </summary>
    public enum PlayerState {
        Stopped = 0,
        Playing = 1,
        Paused = 2
    }

    public static class StateConverter {
        private static Tuple<PlayerState, PlaybackState>[] Mapping;

        static StateConverter() {
            Mapping = new Tuple<PlayerState, PlaybackState>[] {
                new Tuple<PlayerState, PlaybackState>(PlayerState.Stopped, PlaybackState.Stopped),
                new Tuple<PlayerState, PlaybackState>(PlayerState.Playing, PlaybackState.Playing),
                new Tuple<PlayerState, PlaybackState>(PlayerState.Paused, PlaybackState.Paused)
            };
        }

        public static PlayerState Convert(PlaybackState inState) {
            return Mapping.First(map => map.Item2 == inState).Item1;
        }

        public static PlaybackState Convert(PlayerState inState) {
            return Mapping.First(map => map.Item1 == inState).Item2;
        }

        public static PlayerState? Convert(PlaybackState? inState) {
            if(!inState.HasValue) {
                return null;
            } else {
                return Convert(inState);
            }
        }

        public static PlaybackState? Convert(PlayerState? inState) {
            if(!inState.HasValue) {
                return null;
            } else {
                return Convert(inState);
            }
        }
    }
}