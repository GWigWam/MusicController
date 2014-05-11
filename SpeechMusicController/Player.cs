using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {
    class Player {
        private Process aimp3 = new Process();

        public Player(string playerLoc) {
            aimp3.StartInfo.FileName = playerLoc;
        }

        public void Play(string fileLocation) {
            aimp3.StartInfo.Arguments = "/ADD_PLAY " + fileLocation;
            aimp3.Start();
        }

        public void Toggle() {
            aimp3.StartInfo.Arguments = "/PAUSE";
            aimp3.Start();
        }

        public void Next() {
            aimp3.StartInfo.Arguments = "/NEXT";
            aimp3.Start(); 
        }

        public void Previous() {
            aimp3.StartInfo.Arguments = "/PREV";
            aimp3.Start();
        }
    }
}
