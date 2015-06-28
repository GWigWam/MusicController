using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace SpeechMusicController {
    public class Aimp3Player : IPlayer, IDisposable {
        private Process aimp3;
        private const int MaxCharInFilename = 32000; //32,768 actually, but keep some headroom

        public Aimp3Player(string playerLoc) {
            aimp3 = new Process();
            aimp3.StartInfo.FileName = playerLoc;
        }

        public void Play(Uri song) {
            //Add " at begin and end so AIMP3 gets it
            aimp3.StartInfo.Arguments = string.Format("/ADD_PLAY \"{0}\"", song.LocalPath);
            aimp3.Start();
        }

        public async void Play(IEnumerable<Uri> playList) {
            if (playList.Count() >= 1) {
                //Play 1st song
                Play(playList.First());

                //After a small delay add other songs in playList to AIMP3s playlist
                if (playList.Count() > 1) {
                    //Delay shouldn't halt thread
                    await Task.Run(() => {
                        //Alway wait 1s before adding new songs
                        Task.Delay(1000).Wait();
                        //Make sure program is fired up properly before adding more songs
                        for (int tries = 0; tries < 20; tries++) {
                            Process[] found = Process.GetProcessesByName("AIMP3");
                            if (found.Length >= 1 && DateTime.Now - found[0].StartTime > TimeSpan.FromSeconds(5)) {
                                Insert(playList.Skip(1));
                                break;
                            } else {
                                Task.Delay(2500).Wait();
                            }
                        }
                    });
                }
            }
        }

        private void Insert(IEnumerable<Uri> inserList) {
            string insertString = "/INSERT ";
            var insertLater = new List<Uri>();

            foreach (Uri curSong in inserList) {
                if (insertString.Length < MaxCharInFilename) {
                    insertString += string.Format("\"{0}\" ", curSong.LocalPath);
                } else {
                    insertLater.Add(curSong);
                }
            }

            aimp3.StartInfo.Arguments = insertString;
            aimp3.Start();

            if (insertLater.Count > 0) {
                Insert(insertLater);
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
            if (aimp3 != null) {
                aimp3.Dispose();
            }
        }
    }
}