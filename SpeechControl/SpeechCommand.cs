using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechControl {

    public class SpeechCommand {
        public List<IEnumerable<string>> KeyWords { get; set; }

        public string Description {
            get; set;
        }

        public ExecuteCommand Execute {
            private get; set;
        }

        public SpeechCommand() {
            KeyWords = new List<IEnumerable<string>>();
        }

        public Func<bool> CanExecute {
            get; set;
        }

        public bool IsMatch(IEnumerable<string> sentence) {
            if(sentence.Count() != KeyWords.Count)
                return false;

            for(int i = 0; i < KeyWords.Count; i++) {
                if(!KeyWords.ElementAt(i).Contains(sentence.ElementAt(i)))
                    return false;
            }

            return true;
        }

        public IEnumerable<string> ExecuteCommand(IEnumerable<string> sentence) {
            // Rare speciment where '!= false' is NOT same as '== true'
            //        | != false    |   == true  |
            //--------|-------------|------------|
            //  true  |  true       |    true    |
            //  false |  false      |    false   |
            //  null  |  true       |    false   |
            //-----------------------------------|
            if(CanExecute?.Invoke() != false) {
                return Execute?.Invoke(sentence) ?? new string[0];
            } else {
                return new string[0];
            }
        }

        public static SpeechCommand[] CreateCommands(SpeechController speechController) {
            return new SpeechCommand[] {
                new SpeechCommand() {
                    KeyWords = new List<IEnumerable<string>>() {
                        new string[] { "music" },
                        new string[] { "volume up", "volume down" }
                    },
                    Description = "Music + [Volume up / Volume down]+ : Change volume",
                    Execute = (sentence) => {
                        if(sentence.ElementAt(1) == "volume up") {
                            speechController.Settings.Volume += 0.1f; //TODO: Get amount from settings
                        } else if(sentence.ElementAt(1) == "volume down") {
                            speechController.Settings.Volume -= 0.1f; //TODO: Get amount from settings
                        }
                        return new string[] { "music" };
                    },
                    CanExecute = () => speechController.Settings.EnableSpeech
                },

                new SpeechCommand() {
                    KeyWords = CreateSingleChoiceKeywords("music", "switch"),
                    Description = "Music + Switch : Pause/Unpause",
                    Execute = (sentence) => {
                        speechController.Player.TogglePause(true);
                        return new string[0];
                    },
                    CanExecute = () => speechController.Settings.EnableSpeech
                },

                new SpeechCommand() {
                    KeyWords = new List<IEnumerable<string>>() {
                        new string[] { "music" },
                        new string[] { "next", "previous" }
                    },
                    Description = "Music + [Next / Previous]+ : Step through playlist",
                    Execute = (sentence) => {
                        var dir = sentence.ElementAt(1);
                        int increment = (dir == "next") ? 1 : (dir == "previous") ? -1 : 0;
                        speechController.Playlist.CurrentSongIndex += increment;
                        return new string[] { "music" };
                    },
                    CanExecute = () => speechController.Settings.EnableSpeech
                },

                new SpeechCommand() {
                    KeyWords = new List<IEnumerable<string>>() {
                        new string[] { "music" },
                        new string[] { "random", "shuffle" }
                    },
                    Description = "Music + [Random / Shuffle] : Shuffle the playlist",
                    Execute = (sentence) => {
                        speechController.Playlist.Shuffle();
                        return new string[0];
                    },
                    CanExecute = () => speechController.Settings.EnableSpeech
                },

                new SpeechCommand() {
                    KeyWords = new List<IEnumerable<string>>() {
                        new string[] { "music" },
                        new string[] { "stop listening", "start listening" }
                    },
                    Description = "Music + [Stop_listening / Start_listening] : Enable or disable speech input",
                    Execute = (sentence) => {
                        speechController.Settings.EnableSpeech = sentence.ElementAt(1) == "start listening";
                        return new string[0];
                    }
                },

                #region play song/album/artist
                new SpeechCommand() {
                    KeyWords = new List<IEnumerable<string>>() {
                        new string[] { "play song" },
                        speechController.Playlist.Select(s => s.Title)
                    },
                    Description = "Play_song + <Song Title> : Play songs by name",
                    Execute = (sentence) => {
                        var songName = sentence.ElementAt(1);
                        speechController.Playlist.PlayAllMatches(s => s?.Title?.Equals(songName, StringComparison.CurrentCultureIgnoreCase) ?? false);
                        return new string[0];
                    },
                    CanExecute = () => speechController.Settings.EnableSpeech
                },

                new SpeechCommand() {
                    KeyWords = new List<IEnumerable<string>>() {
                        new string[] { "play album" },
                        speechController.Playlist.Select(s => s.Album)
                    },
                    Description = "Play_album + <Album Name> : Play songs by album",
                    Execute = (sentence) => {
                        var albumName = sentence.ElementAt(1);
                        speechController.Playlist.PlayAllMatches(s => s?.Album?.Equals(albumName, StringComparison.CurrentCultureIgnoreCase) ?? false);
                        return new string[0];
                    },
                    CanExecute = () => speechController.Settings.EnableSpeech
                },

                new SpeechCommand() {
                    KeyWords = new List<IEnumerable<string>>() {
                        new string[] { "play artist" },
                        speechController.Playlist.Select(s => s.Artist)
                    },
                    Description = "Play_artist + <Arist Name> : Play songs by artist",
                    Execute = (sentence) => {
                        var artistName = sentence.ElementAt(1);
                        speechController.Playlist.PlayAllMatches(s => s?.Artist?.Equals(artistName, StringComparison.CurrentCultureIgnoreCase) ?? false);
                        return new string[0];
                    },
                    CanExecute = () => speechController.Settings.EnableSpeech
                },
                #endregion play song/album/artist
            };
        }

        private static List<IEnumerable<string>> CreateSingleChoiceKeywords(params string[] keywords) {
            var retList = new List<IEnumerable<string>>();
            foreach(var keyword in keywords) {
                retList.Add(new string[] { keyword });
            }
            return retList;
        }
    }

    public static class SpeechCommandsExtensions {

        public static IEnumerable<string> GetAllKeywords(this IEnumerable<SpeechCommand> commands) {
            return new HashSet<string>(commands.SelectMany(sc => sc.KeyWords.SelectMany(s => s)).Where(s => !string.IsNullOrEmpty(s)));
        }
    }

    /// <summary>
    /// The function to executed when a command is spoken
    /// </summary>
    /// <param name="sentence">Spoken input</param>
    /// <returns>Spoken input cache to use after command is stated</returns>
    public delegate IEnumerable<string> ExecuteCommand(IEnumerable<string> sentence);
}