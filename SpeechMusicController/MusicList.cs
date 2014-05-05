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
            scanDir(dirLoc);
            fillTuple();
        }

        private List<FileInfo> AllFiles = new List<FileInfo>();

        //First string is Song name, second string is full location
        List<Tuple<string, string>> NameLocationMap = new List<Tuple<string, string>>();

        private void scanDir(string dirLoc) {
            DirectoryInfo dir = new DirectoryInfo(dirLoc);
            Console.WriteLine("Directory {0}", dir.FullName);
            // list the files
            try {
                foreach(FileInfo f in dir.GetFiles("*.mp3")) {
                    //Console.WriteLine("File {0}", f.Name);
                    AllFiles.Add(f);
                }
            } catch {
                Console.WriteLine("Directory {0}  \n could not be accessed!!!!", dir.FullName);
                return;  // We alredy got an error trying to access dir so dont try to access it again
            }

            // process each directory
            foreach(DirectoryInfo d in dir.GetDirectories()) {
                scanDir(d.FullName);
            }
        }

        private void fillTuple() {
            foreach(FileInfo fi in AllFiles) { 
                string songName = fi.Name;

                //Strip .mp3
                songName = songName.Substring(0, songName.Length - 4);
                
                //Get rid of '# - ', '# ' (\d means any digit) and ' - ' and of ( & )
                songName = Regex.Replace(songName, @"\d+ - ", "");
                songName = Regex.Replace(songName, @"^\d+.? ?", "");
                songName = Regex.Replace(songName, @" - ", " ");
                songName = Regex.Replace(songName, @"(\(|\))", "");

                NameLocationMap.Add(new Tuple<string, string>(songName, fi.FullName));
            }
        }

        public List<string> getAllSongNames() {
            var returnList = new List<string>();
            for(int c = 0; c < NameLocationMap.Count(); c++) {
                returnList.Add(NameLocationMap[c].Item1);
            }

            return returnList;
        }

        public string getSongLocation(string songName) {
            for(int c = 0; c < NameLocationMap.Count(); c++) {
                if(NameLocationMap[c].Item1.Equals(songName)) {
                    return NameLocationMap[c].Item2;
                }
            }

            throw new Exception("Song with the name " + songName + " could not be found");
        }
    }
}
