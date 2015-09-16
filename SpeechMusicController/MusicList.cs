using SpeechMusicController.AppSettings;
using SpeechMusicController.AppSettings.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpeechMusicController {

    public class MusicList {

        //All songs as read from disc
        private Song[] InternalSongList;

        private Random Random;
        private SongRules Rules;

        public MusicList(SongRules rules) {
            Random = new Random();
            ReadListFromDisc();

            if(rules != null) {
                Rules = rules;
            } else {
                throw new ArgumentNullException(nameof(rules));
            }
        }

        public event Action SongListUpdated;

        //Songs after rules have been applied to them
        public IEnumerable<Song> ActiveSongs => RemoveDuplicates(ApplyRules(InternalSongList));

        public IEnumerable<Song> AllSongs => RemoveDuplicates(InternalSongList);

        public string[] GetAllSongKeywords() {
            var keywordList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            //Add everything to list
            foreach(Song song in ActiveSongs) {
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

        public Song[] GetMatchingSongs(string keyword, bool matchOnTitle = true, bool matchOnArtist = true, bool matchOnAlbum = true) {
            if(matchOnTitle || matchOnArtist || matchOnAlbum) {
                //Get matches from active songs
                IEnumerable<Song> retList = ActiveSongs.Where(s =>
                    (matchOnTitle && string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase)) ||
                    (matchOnArtist && string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase)) ||
                    (matchOnAlbum && string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase))
                );

                if(retList.Count() == 0) {
                    //If no matches in active, search for matches in all songs
                    retList = AllSongs.Where(s =>
                        (matchOnTitle && string.Equals(s.Title, keyword, StringComparison.InvariantCultureIgnoreCase)) ||
                        (matchOnArtist && string.Equals(s.Artist, keyword, StringComparison.InvariantCultureIgnoreCase)) ||
                        (matchOnAlbum && string.Equals(s.Album, keyword, StringComparison.InvariantCultureIgnoreCase))
                    );
                }

                //Shuffle & return
                return retList.OrderBy(s => Random.Next()).ToArray();
            } else {
                return new Song[0];
            }
        }

        public Song GetRandomSong() => ActiveSongs.ElementAt(Random.Next(0, ActiveSongs.Count()));

        public void ReadListFromDisc() {
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

        private List<Song> ApplyRules(IEnumerable<Song> songs) {
            List<Song> retList = new List<Song>(songs);

            foreach(SongRule rule in Rules.GetSongRules(true, true)) {
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

        private IEnumerable<Song> OrganizeList(FileInfo[] allFiles) {
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

        private IEnumerable<Song> RemoveDuplicates(IEnumerable<Song> songs) => new HashSet<Song>(songs, new SongComparerByTitleAndArtist());

        private IEnumerable<FileInfo> ScanDir(string dirLoc) {
            var dir = new DirectoryInfo(dirLoc);
            if(dir.Exists) {
                //Console.WriteLine("Directory {0}", dir.FullName);
                // list the files
                foreach(FileInfo f in dir.GetFiles("*.mp3", SearchOption.AllDirectories)) {
                    yield return f;
                }
            }
        }
    }
}