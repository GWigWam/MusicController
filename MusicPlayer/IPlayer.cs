using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeechMusicController {

    public interface IPlayer {

        void Play(Uri song);

        void Play(IEnumerable<Uri> playList);

        void Play();

        void Toggle();

        void Next();

        void Previous();

        void VolUp();

        void VolDown();
    }
}