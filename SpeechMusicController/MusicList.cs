using SpeechMusicController.AppSettings;
using SpeechMusicController.AppSettings.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpeechMusicController {
    internal static class MusicList {
        private static List<FileInfo> AllFiles;
        private static Random random;

        //All songs as read from disc
        private static List<Song> InternalSongList;

        public static event Action SongListUpdated;

        //Songs after rules have been applied to them
        public static List<Song> ActiveSongs {
            get {
                return RemoveDuplicates(ApplyRules(InternalSongList));
            }
        }

        static MusicList() {
            ReadListFromDisc();
        }

        public static void ReadListFromDisc() {
            AllFiles = new List<FileInfo>();
            random = new Random();
            InternalSongList = new List<Song>();

            var dirLoc = Settings.Instance.GetSetting("MusicFolder");
            if (!string.IsNullOrEmpty(dirLoc)) {
                ScanDir(dirLoc);
                OrganizeList();

                if (SongListUpdated != null) {
                    SongListUpdated();
                }
            } else {
                System.Windows.Forms.MessageBox.Show("Error: MusicFolder setting is empty");
            }
        }

        private static void ScanDir(string dirLoc) {
            DirectoryInfo dir = new DirectoryInfo(dirLoc);
            if (dir.Exists) {
                //Console.WriteLine("Directory {0}", dir.FullName);
                // list the files
                try {
                    //Add .mp3 files
                    foreach (FileInfo f in dir.GetFiles("*.mp3")) {
                        //Console.WriteLine("File {0}", f.Name);
                        AllFiles.Add(f);
                    }
                } catch { }

                // process each directory
                foreach (DirectoryInfo d in dir.GetDirectories()) {
                    ScanDir(d.FullName);
                }
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

        private static List<Song> RemoveDuplicates(List<Song> songs) {
            var cleanList = new HashSet<Song>(songs);
            return cleanList.ToList();
        }

        private static List<Song> ApplyRules(List<Song> songs) {
            List<Song> tmpList = new List<Song>(songs);

            foreach (SongRule rule in Settings.Instance.GetSongRules(true, true)) {
                if (rule.Type == SongRuleType.Exclude) {
                    tmpList.RemoveAll(curSong => rule.Attributes == curSong.Attributes);
                } else if (rule.Type == SongRuleType.NameChange) {
                    var song = tmpList.FirstOrDefault(curSong => rule.Attributes == curSong.Attributes);
                    if (!song.Equals(default(Song))) {
                        tmpList.Remove(song);
                        tmpList.Add(new Song(((NameChangeRule)rule).NewName, song.Attributes.Artist, song.Attributes.Album, song.FilePath));
                    }
                }
            }

            return tmpList;
        }

        public static string[] GetAllSongKeywords() {
            var keywordList = new HashSet<string>();

            //Add everything to list
            foreach (Song song in ActiveSongs) {
                if (!string.IsNullOrEmpty(song.Attributes.Album)) keywordList.Add(song.Attributes.Album);
                if (!string.IsNullOrEmpty(song.Attributes.Artist)) keywordList.Add(song.Attributes.Artist);
                if (!string.IsNullOrEmpty(song.Attributes.Title)) keywordList.Add(song.Attributes.Title);
            }

            return keywordList.ToArray();
        }

        public static IEnumerable<Song> GetMatchingSongs(string keyword) {
            IEnumerable<Song> retList = ActiveSongs.Where(s =>
                string.Equals(s.Attributes.Album, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Attributes.Artist, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Attributes.Title, keyword, StringComparison.InvariantCultureIgnoreCase));

            retList = retList.OrderByDescending(s => {
                if (string.Equals(s.Attributes.Title, keyword, StringComparison.InvariantCultureIgnoreCase)) return 1;
                if (string.Equals(s.Attributes.Artist, keyword, StringComparison.InvariantCultureIgnoreCase)) return 0;
                if (string.Equals(s.Attributes.Album, keyword, StringComparison.InvariantCultureIgnoreCase)) return -1;
                return -1;
            });

            return retList;
        }

        public static Song GetRandomSong() {
            return ActiveSongs[random.Next(0, ActiveSongs.Count)];
        }
    }
}