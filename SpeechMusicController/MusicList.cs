using SpeechMusicController.AppSettings;
using SpeechMusicController.AppSettings.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpeechMusicController {

    internal static class MusicList {
        private static Random random;

        //All songs as read from disc
        private static Song[] InternalSongList;

        public static event Action SongListUpdated;

        //Songs after rules have been applied to them
        public static IEnumerable<Song> ActiveSongs => RemoveDuplicates(ApplyRules(InternalSongList));

        public static IEnumerable<Song> AllSongs => RemoveDuplicates(InternalSongList);

        static MusicList() {
            random = new Random();
            ReadListFromDisc();
        }

        public static void ReadListFromDisc() {
            InternalSongList = new Song[0];

            var dirLoc = Settings.Instance.GetSetting("MusicFolder");
            if(!string.IsNullOrEmpty(dirLoc)) {
                FileInfo[] allFiles = ScanDir(dirLoc).ToArray();
                InternalSongList = OrganizeList(allFiles).ToArray();

                SongListUpdated?.Invoke();
            } else {
                System.Windows.Forms.MessageBox.Show("Error: MusicFolder setting is empty");
            }
        }

        private static IEnumerable<FileInfo> ScanDir(string dirLoc) {
            var dir = new DirectoryInfo(dirLoc);
            if(dir.Exists) {
                //Console.WriteLine("Directory {0}", dir.FullName);
                // list the files
                foreach(FileInfo f in dir.GetFiles("*.mp3", SearchOption.AllDirectories)) {
                    yield return f;
                }
            }
        }

        private static IEnumerable<Song> OrganizeList(FileInfo[] allFiles) {
            List<Song> OrganizedList = new List<Song>();
            foreach(FileInfo fi in allFiles) {
                var fileProps = TagLib.File.Create(fi.FullName);

                var propTitle = string.IsNullOrWhiteSpace(fileProps.Tag.Title) ? null : fileProps.Tag.Title;
                var propArtist = string.IsNullOrWhiteSpace(fileProps.Tag.FirstPerformer) ? null : fileProps.Tag.FirstPerformer;
                var propAlbum = string.IsNullOrWhiteSpace(fileProps.Tag.Album) ? null : fileProps.Tag.Album;

                //Strip .mp3
                var fileTitle = fi.Name.Substring(0, fi.Name.Length - 4);

                string fileArtist = null;
                if(fileTitle.Contains(" - ")) {
                    fileArtist = fileTitle.Substring(0, fileTitle.IndexOf(" - ")).Trim();
                    fileTitle = fileTitle.Substring(fileTitle.IndexOf(" - ") + 3).Trim();
                }

                Song song = new Song(
                    propTitle == null ? Regex.Replace(fileTitle, @"(\(|\))", "").Trim() : Regex.Replace(propTitle, @"(\(|\))", "").Trim(),
                    propArtist ?? fileArtist,
                    propAlbum,
                    fi.FullName);
                yield return song;
            }
        }

        private static IEnumerable<Song> RemoveDuplicates(IEnumerable<Song> songs) => new HashSet<Song>(songs, new SongComparerByTitleAndArtist());

        private static List<Song> ApplyRules(IEnumerable<Song> songs) {
            List<Song> retList = new List<Song>(songs);

            foreach(SongRule rule in Settings.Instance.GetSongRules(true, true)) {
                if(rule.Type == SongRuleType.Exclude) {
                    retList.RemoveAll(curSong => rule.Attributes == curSong.Attributes);
                } else if(rule.Type == SongRuleType.NameChange) {
                    var song = retList.FirstOrDefault(curSong => rule.Attributes == curSong.Attributes);
                    if(song != default(Song)) {
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
            foreach(Song song in AllSongs) {
                if(!string.IsNullOrEmpty(song.Album))
                    keywordList.Add(song.Album);
                if(!string.IsNullOrEmpty(song.Artist))
                    keywordList.Add(song.Artist);
                if(!string.IsNullOrEmpty(song.Title))
                    keywordList.Add(song.Title);
            }

            //return keywordList.Select(s => Regex.Replace(s, @"\(|\)", "").Trim()).ToArray();
            return keywordList.ToArray();
        }

        public static IEnumerable<Song> GetMatchingSongs(string keyword) {
            //Get matches from active songs
            IEnumerable<Song> retList = ActiveSongs.Where(s =>
                string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase)
            );

            if(retList.Count() == 0) {
                //If no matches in active, search for matches in all songs
                retList = AllSongs.Where(s =>
                    string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase)
                );
            }

            //Shuffle
            retList = retList.OrderBy(s => random.Next());

            //Sort by relevance
            retList = retList.OrderByDescending(s => {
                if(string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase))
                    return 1;
                if(string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase))
                    return 0;
                if(string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase))
                    return -1;
                return -1;
            });

            return retList;
        }

        public static Song GetRandomSong() => ActiveSongs.ElementAt(random.Next(0, ActiveSongs.Count()));
    }
}