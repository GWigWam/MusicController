using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeechMusicController {

    public interface IPlayer {

        void Play(string songPath);

        void Play(IEnumerable<string> songPaths);

        void Play();

        void Toggle();

        void Next();

        void Previous();

        void VolUp();

        void VolDown();
    }
}