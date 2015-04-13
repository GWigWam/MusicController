using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace SpeechMusicController {
    internal class Aimp3Player : IPlayer, IDisposable {
        private Process aimp3;
        private const int MaxCharInFilename = 32000; //32,768 actually, but keep some headroom

        public Aimp3Player(string playerLoc) {
            aimp3 = new Process();
            aimp3.StartInfo.FileName = playerLoc;
        }

        //Play single song
        public void Play(Song song) {
            aimp3.StartInfo.Arguments = string.Format("/ADD_PLAY \"{0}\"", song.FilePath);//Add " at begin and end so AIMP3 gets it
            aimp3.Start();
        }

        public void Play(IEnumerable<Song> playList) {
            if (playList.Count() == 1) {
                Play(playList.First());
            } else if (playList.Count() > 1) {
                Play(playList.First());

                Insert(playList.Skip(1).ToList(), false);
            }
        }

        private void Insert(IEnumerable<Song> inserList, bool callPlay) {
            string insertString = "/INSERT ";
            List<Song> insertLater = new List<Song>();
            for (int c = 0; c < inserList.Count(); c++) {
                if (insertString.Length < MaxCharInFilename) {
                    insertString += string.Format("\"{0}\" ", inserList.ElementAt(c).FilePath);
                } else {
                    insertLater.Add(inserList.ElementAt(c));
                }
            }
            aimp3.StartInfo.Arguments = insertString;
            aimp3.Start();

            if (insertLater.Count > 0) {
                Insert(insertLater, false);
            }

            if (callPlay) {
                //Gives Aimp3 time to load, then calls play
                Timer playTimer = new Timer(3000);
                playTimer.Elapsed += new ElapsedEventHandler((s, e) => Play());
                playTimer.AutoReset = false; //Only get called once
                playTimer.Enabled = true;
            }
        }

        public void Play() {
            aimp3.StartInfo.Arguments = "/PLAY";
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

        public void VolUp() {
            aimp3.StartInfo.Arguments = "/VOLUP";
            aimp3.Start();
        }

        public void VolDown() {
            aimp3.StartInfo.Arguments = "/VOLDWN";
            aimp3.Start();
        }

        public void Dispose() {
            try {
                aimp3.Dispose();
            } catch { }
        }
    }
}