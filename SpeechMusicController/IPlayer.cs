using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeechMusicController {
    internal interface IPlayer {
        void Play(Song song);
        void Play(IEnumerable<Song> playList);
        void Play();

        void Toggle();
        void Next();
        void Previous();
        void VolUp();
        void VolDown();
    }
}
