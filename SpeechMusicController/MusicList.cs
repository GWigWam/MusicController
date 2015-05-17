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
        private static Song[] InternalSongList;

        public static event Action SongListUpdated;

        //Songs after rules have been applied to them
        public static IEnumerable<Song> ActiveSongs {
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
            InternalSongList = new Song[0];

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
            List<Song> OrganizedList = new List<Song>();
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
                OrganizedList.Add(song);
            }
            InternalSongList = OrganizedList.ToArray();
        }

        private static IEnumerable<Song> RemoveDuplicates(IEnumerable<Song> songs) {
            return new HashSet<Song>(songs);
        }

        private static List<Song> ApplyRules(IEnumerable<Song> songs) {
            List<Song> retList = new List<Song>(songs);

            foreach (SongRule rule in Settings.Instance.GetSongRules(true, true)) {
                if (rule.Type == SongRuleType.Exclude) {
                    retList.RemoveAll(curSong => rule.Attributes == curSong.Attributes);
                } else if (rule.Type == SongRuleType.NameChange) {
                    var song = retList.FirstOrDefault(curSong => rule.Attributes == curSong.Attributes);
                    if (song != default(Song)) {
                        retList.Remove(song);
                        retList.Add(new Song(((NameChangeRule)rule).NewName, song.Artist, song.Album, song.FilePath));
                    }
                }
            }

            return retList;
        }

        public static string[] GetAllSongKeywords() {
            var keywordList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            //Add everything to list
            foreach (Song song in ActiveSongs) {
                if (!string.IsNullOrEmpty(song.Album)) keywordList.Add(song.Album);
                if (!string.IsNullOrEmpty(song.Artist)) keywordList.Add(song.Artist);
                if (!string.IsNullOrEmpty(song.Title)) keywordList.Add(song.Title);
            }

            return keywordList.ToArray();
        }

        public static IEnumerable<Song> GetMatchingSongs(string keyword) {
            IEnumerable<Song> retList = ActiveSongs.Where(s =>
                string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase));

            retList = retList.OrderByDescending(s => {
                if (string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase)) return 1;
                if (string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase)) return 0;
                if (string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase)) return -1;
                return -1;
            });

            return retList;
        }

        public static Song GetRandomSong() {
            return ActiveSongs.ElementAt(random.Next(0, ActiveSongs.Count()));
        }
    }
}