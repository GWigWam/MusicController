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

        //First string is Song name, second string is full location
        //List<Tuple<string, string>> NameLocationMap = new List<Tuple<string, string>>();
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
            var returnList = new List<string>();

            //Add everything to list
            for(int c = 0; c < SongList.Count(); c++) {
                returnList.Add(SongList[c].SongName);
                returnList.Add(SongList[c].BandName);
            }

            //Clean list of duplicates
            returnList = returnList.Distinct().ToList();

            return returnList;
        }

        public List<Song> GetMatchingSongs(string keyword) {
            List<Song> returnList = new List<Song>();

            for(int c = 0; c < SongList.Count(); c++) {
                Song current = SongList[c];

                if(current.SongName.Equals(keyword) || current.BandName.Equals(keyword)) {
                    returnList.Add(current);
                }
            }

            return returnList;
        }

        public Song GetRandomSong() {
            return SongList[random.Next(0, SongList.Count)];
        }
    }
}
