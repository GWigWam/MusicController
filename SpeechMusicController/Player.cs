using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {
    class Player {
        private Process aimp3 = new Process();

        public Player() {
            aimp3.StartInfo.FileName = getAimp3Location();
        }

        private string getAimp3Location(){
            return @"F:\Program Files (x86)\AIMP3\AIMP3.exe";
        }

        public void play(string fileLocation) {
            aimp3.StartInfo.Arguments = "/ADD_PLAY " + fileLocation;
            aimp3.Start();
        }
    }
}
