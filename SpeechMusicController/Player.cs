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

        //Play single song
        public void Play(Song song) {
            aimp3.StartInfo.Arguments = "/ADD_PLAY " + "\"" + song.FilePath + "\"";//Add " at begin and end so AIMP3 gets it
            aimp3.Start();
        }

        public void Play(List<Song> playList) {
            string playString = "/FILE ";
            for(int c = 0; c < playList.Count(); c++) {
                playString += "\"" + playList[c].FilePath + "\" ";
            }
            aimp3.StartInfo.Arguments = playString;
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
