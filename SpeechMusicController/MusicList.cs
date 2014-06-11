using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpeechMusicController {
    class MusicList {

        public MusicList(string dirLoc) {
            ScanDir(dirLoc);
            OrganizeList();
        }

        private List<FileInfo> AllFiles = new List<FileInfo>();
        private Random random = new Random();

        public List<Song> SongList = new List<Song>();

        private void ScanDir(string dirLoc) {
            DirectoryInfo dir = new DirectoryInfo(dirLoc);
            //Console.WriteLine("Directory {0}", dir.FullName);
            // list the files
            try {
                foreach(FileInfo f in dir.GetFiles("*.mp3")) {
                    //Console.WriteLine("File {0}", f.Name);
                    AllFiles.Add(f);
                }
            } catch {
                Console.WriteLine("Directory " + dir.FullName + " \n could not be accessed!!!!");
                return;
            }

            // process each directory
            foreach(DirectoryInfo d in dir.GetDirectories()) {
                ScanDir(d.FullName);
            }
        }

        private void OrganizeList() {
            foreach(FileInfo fi in AllFiles) {
                string songName = fi.Name;
                //Strip .mp3
                songName = songName.Substring(0, songName.Length - 4);

                //Get rid of '# - ', '# ' (\d means any digit) and of '(' & ')'
                songName = Regex.Replace(songName, @"\d+ - ", "");
                songName = Regex.Replace(songName, @"^\d+.? ?", "");
                songName = Regex.Replace(songName, @"(\(|\))", "");

                if(songName.Contains(" - ")) {
                    string songArtist = songName.Substring(0, songName.IndexOf(" - "));
                    string songTitle = songName.Substring(songName.IndexOf(" - ") + 3);

                    Song song = new Song(songTitle, songArtist, fi.FullName);
                    SongList.Add(song);
                }
            }
        }

        public List<string> GetAllSongAndBandNames() {
            var tmpList = new List<string>();

            //Add everything to list
            for(int c = 0; c < SongList.Count(); c++) {
                tmpList.Add(SongList[c].SongName);
                tmpList.Add(SongList[c].BandName);
            }

            //Clean list of duplicates
            var returnList = new List<string>();
            foreach(string s1 in tmpList) {
                bool isNew = true;
                foreach(string s2 in returnList) {
                    if(s1.Equals(s2)) {
                        isNew = false;
                    }
                }
                if(isNew) {
                    returnList.Add(s1);
                }
            }
            return returnList;
        }

        public List<Song> GetMatchingSongs(string keyword) {
            List<Song> matchingSongs = new List<Song>();

            for(int c = 0; c < SongList.Count(); c++) {
                Song current = SongList[c];

                if(current.SongName.Equals(keyword) || current.BandName.Equals(keyword)) {
                    matchingSongs.Add(current);
                }
            }
            
            //Clean list of duplicates, even if they are indeed seperate files in differnt folders, but the same song
            List<Song> returnList = new List<Song>();
            foreach(Song s1 in matchingSongs) {
                bool isNew = true;
                foreach(Song s2 in returnList) {
                    if(s1.SongName.Equals(s2.SongName) && s1.BandName.Equals(s2.BandName)) {
                        isNew = false;
                    }
                }
                if(isNew) {
                    returnList.Add(s1);
                }
            }
            return returnList;
        }

        public Song GetRandomSong() {
            return SongList[random.Next(0, SongList.Count)];
        }
    }
}
