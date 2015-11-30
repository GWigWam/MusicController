using SpeechMusicController.AppSettings;
using SpeechMusicController.AppSettings.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TagLib;

namespace SpeechMusicController {

    public class MusicList {

        //All songs as read from disc
        private Song[] InternalSongList;

        private Random Random;
        private SongRules Rules;
        private Settings AppSettings;

        private readonly Regex SongNameInfo = new Regex(@"^\s*(?<artist>.+?) - (?<title>.+?)(?<extension>\.[a-z]\S*)$");
        private readonly Regex Parenthesis = new Regex(@"\(|\)");

        public MusicList(Settings settings, SongRules rules) {
            if(rules != null) {
                Rules = rules;
            } else {
                throw new ArgumentNullException(nameof(rules));
            }
            AppSettings = settings;
            Random = new Random();

            ReadListFromDisc();

            Rules.OnChange += (s, a) => {
                FillSongLists();
            };
        }

        private void FillSongLists() {
            ActiveSongs = RemoveDuplicates(ApplyRules(InternalSongList));
            AllSongs = RemoveDuplicates(InternalSongList);
        }

        public event Action SongListUpdated;

        //Songs after rules have been applied to them
        public IEnumerable<Song> ActiveSongs {
            get; private set;
        }

        public IEnumerable<Song> AllSongs {
            get; private set;
        }

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

            string dirLoc;
            if(AppSettings.TryGetSetting("MusicFolder", out dirLoc) && !string.IsNullOrEmpty(dirLoc)) {
                FileInfo[] allFiles = ScanDir(dirLoc).ToArray();
                InternalSongList = OrganizeList(allFiles).ToArray();

                FillSongLists();
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

        private IEnumerable<Song> OrganizeList(FileInfo[] files) {
            foreach(FileInfo curFile in files) {
                Tag tag = null;
                try {
                    tag = TagLib.File.Create(curFile.FullName).Tag;
                } catch { }
                Match matchName = null;

                string title = tag?.Title ?? (matchName ?? (matchName = SongNameInfo.Match(curFile.Name))).Groups?["title"]?.Value;
                string artist = tag?.FirstPerformer ?? (matchName ?? (matchName = SongNameInfo.Match(curFile.Name))).Groups?["artist"]?.Value;
                string album = tag?.Album;

                if(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(artist)) {
                    //Remove parenthesis '(' & ')'
                    title = Parenthesis.Replace(title, "").Trim();
                    artist = Parenthesis.Replace(artist, "").Trim();
                    album = album?.Trim();
                    album = string.IsNullOrWhiteSpace(album) ? null : album;

                    var song = new Song(title, artist, album, curFile.FullName);
                    yield return song;
                }
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