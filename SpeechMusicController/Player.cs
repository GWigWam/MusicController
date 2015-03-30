using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SpeechMusicController {

    internal class Player {
        private Process aimp3 = new Process();
        private const int MaxCharInFilename = 32000; //32,768 actually, but keep some headroom

        public Player(string playerLoc) {
            aimp3.StartInfo.FileName = playerLoc;
        }

        //Play single song
        public void Play(Song song) {
            aimp3.StartInfo.Arguments = "/ADD_PLAY " + "\"" + song.FilePath + "\"";//Add " at begin and end so AIMP3 gets it
            aimp3.Start();
        }

        public void Play(Song[] playList, bool PlayNr1First = true) {
            if (playList.Length == 1) {
                Play(playList.First());
            } else if (playList.Length > 1) {
                if (PlayNr1First) {
                    Play(playList.First());
                    Play(playList.Skip(1).ToArray(), false);
                }

                string playString = "/FILE ";
                List<Song> insertLater = new List<Song>();
                for (int c = 0; c < playList.Count(); c++) {
                    if (playString.Count() < MaxCharInFilename) {
                        playString += "\"" + playList[c].FilePath + "\" ";
                    } else {
                        insertLater.Add(playList[c]);
                    }
                }
                aimp3.StartInfo.Arguments = playString;
                aimp3.Start();

                if (insertLater.Count > 0) {
                    Insert(insertLater);
                }
            }
        }

        private void Insert(List<Song> inserList) {
            string insertString = "/INSERT ";
            List<Song> insertLater = new List<Song>();
            for (int c = 0; c < inserList.Count(); c++) {
                if (inserList.Count() < MaxCharInFilename) {
                    insertString += "\"" + inserList[c].FilePath + "\" ";
                } else {
                    insertLater.Add(inserList[c]);
                }
            }
            aimp3.StartInfo.Arguments = insertString;
            aimp3.Start();

            if (insertLater.Count > 0) {
                Insert(insertLater);
            }

            //Gives Aimp3 time to load, then calls play
            Timer playTimer = new Timer(3000);
            playTimer.Elapsed += new ElapsedEventHandler(PlayTimerEnds);
            playTimer.AutoReset = false; //Only get called once
            playTimer.Enabled = true;
        }

        private void PlayTimerEnds(object sender, ElapsedEventArgs e) {
            Play();
        }

        //Obsolete?
        /*public void PlayAll() {
            //Not in use now, maybe needed later
            aimp3.StartInfo.Arguments = "/ADD_PLAY " + "\"" + Settings.ReadMusicLocation() + "\"";
            aimp3.Start();
        }*/

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
    }
}