using SpeechMusicController.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpeechMusicController {

    internal static class MusicList {

        static MusicList() {
            var dirLoc = PathSettings.ReadMusicLocation();
            ScanDir(dirLoc);
            OrganizeList();
        }

        private static List<FileInfo> AllFiles = new List<FileInfo>();
        private static Random random = new Random();

        //All songs as read from disc
        private static List<Song> InternalSongList = new List<Song>();

        //Songs after rules have been applied to them
        public static List<Song> ActiveSongs {
            get {
                return ApplyRules(InternalSongList);
            }
        }

        private static void ScanDir(string dirLoc) {
            DirectoryInfo dir = new DirectoryInfo(dirLoc);
            //Console.WriteLine("Directory {0}", dir.FullName);
            // list the files
            try {
                //Add .mp3 files
                foreach (FileInfo f in dir.GetFiles("*.mp3")) {
                    //Console.WriteLine("File {0}", f.Name);
                    AllFiles.Add(f);
                }
            } catch (Exception e) {
                Console.WriteLine("Directory " + dir.FullName + " \n could not be accessed!!!!");
                //return;
            }

            // process each directory
            foreach (DirectoryInfo d in dir.GetDirectories()) {
                ScanDir(d.FullName);
            }
        }

        private static void OrganizeList() {
            foreach (FileInfo fi in AllFiles) {
                var fileProps = TagLib.File.Create(fi.FullName);

                var propTitle = string.IsNullOrWhiteSpace(fileProps.Tag.Title) ? null : fileProps.Tag.Title;
                var propArtist = string.IsNullOrWhiteSpace(fileProps.Tag.FirstPerformer) ? null : fileProps.Tag.FirstPerformer;
                var propAlbum = string.IsNullOrWhiteSpace(fileProps.Tag.Album) ? null : fileProps.Tag.Album;

                //Strip .mp3
                var fileTitle = fi.Name.Substring(0, fi.Name.Length - 4);

                //Get rid of '(' & ')'
                fileTitle = Regex.Replace(fileTitle, @"(\(|\))", "").Trim();

                string fileArtist = null;
                if (fileTitle.Contains(" - ")) {
                    fileArtist = fileTitle.Substring(0, fileTitle.IndexOf(" - ")).Trim();
                    fileTitle = fileTitle.Substring(fileTitle.IndexOf(" - ") + 3).Trim();
                }

                Song song = new Song(
                    propTitle == null ? fileTitle : propTitle,
                    propArtist == null ? fileArtist : propArtist,
                    propAlbum == null ? null : propAlbum, //Technically not necessary
                    fi.FullName);
                InternalSongList.Add(song);
            }
        }

        private static List<Song> ApplyRules(List<Song> songs) {
            List<Song> tmpList = new List<Song>(songs);
            foreach (var rule in SongRules.GetRules(SongRuleType.Exclude)) {
                tmpList.RemoveAll(s => s.GetHashCode() == rule.SongHashCode);
            }

            foreach (var rule in SongRules.GetRules(SongRuleType.NameChange)) {
                var song = tmpList.FirstOrDefault(s => s.GetHashCode() == rule.SongHashCode);
                if (!song.Equals(default(Song))) {
                    tmpList.Remove(song);
                    tmpList.Add(new Song(rule.Parameters, song.Artist, song.Album, song.FilePath));
                }
            }

            return tmpList;
        }

        public static string[] GetAllSongKeywords() {
            var keywordList = new HashSet<string>();

            //Add everything to list
            foreach (Song s in ActiveSongs) {
                if (s.Album != null) keywordList.Add(s.Album);
                if (s.Artist != null) keywordList.Add(s.Artist);
                if (s.Title != null) keywordList.Add(s.Title);
            }

            return keywordList.ToArray();
        }

        public static Song[] GetMatchingSongs(string keyword) {
            var matchingSongs = ActiveSongs.Where(s =>
                string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase));

            var cleanList = new HashSet<Song>(matchingSongs);

            var orderedList = cleanList.OrderByDescending(s => {
                if (string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase)) return 1;
                if (string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase)) return 0;
                if (string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase)) return -1;
                return -1;
            });

            return orderedList.ToArray();
        }

        public static Song GetInternalSongByHash(int hash) {
            return InternalSongList.FirstOrDefault(s => s.GetHashCode() == hash);
        }

        public static Song GetRandomSong() {
            return ActiveSongs[random.Next(0, ActiveSongs.Count)];
        }
    }
}