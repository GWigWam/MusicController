using SpeechMusicController.AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechMusicController {
    public static class SpeechInput {

        public static IEnumerable<string> Keywords {
            get {
                yield return "music";
                foreach (var command in SpeechCommands.Keys) {
                    yield return command;
                }
            }
        }

        public static Dictionary<string, Action> SpeechCommands;

        public static event Action<string> MessageSend;

        private static SpeechRecognitionEngine SRecognize = new SpeechRecognitionEngine();
        private static Random random = new Random();
        private static IPlayer Player;

        static SpeechInput() {
            string path = Settings.Instance.GetSetting("PlayerPath");
            if (!string.IsNullOrEmpty(path)) {
                Player = new Aimp3Player(path);
                InitCommands();
                Start();

                Settings.Instance.OnRulesChanged += LoadGrammar;
            } else {
                System.Windows.Forms.MessageBox.Show("Error: PlayerPath setting is empty");
            }
        }

        private static void InitCommands() {
            SpeechCommands = new Dictionary<string, Action>(){
                { "switch", () => {
                    Player.Toggle();
                    ListeningTimer.Instance.StopListening();
                }},
                { "collection", () => {
                    Player.Play(MusicList.ActiveSongs.OrderBy(s => random.Next()).Select(s => s.FilePath));
                    ListeningTimer.Instance.StopListening();
                }},
                { "full collection", () => {
                    Player.Play(MusicList.AllSongs.OrderBy(s => random.Next()).Select(s => s.FilePath));
                    ListeningTimer.Instance.StopListening();
                }},
                { "random", () => Player.Play(MusicList.GetRandomSong().FilePath) },
                { "next", Player.Next },
                { "previous", Player.Previous },
                { "volume up", Player.VolUp },
                { "volume down", Player.VolDown },
                { "play", Player.Play }
            };
        }

        private static void Start() {
            try {
                LoadGrammar();

                SRecognize.SetInputToDefaultAudioDevice();
                SRecognize.RecognizeAsync(RecognizeMode.Multiple);
            } catch (Exception e) {
                System.Windows.Forms.MessageBox.Show("Error while starting SpeechInput\n" + e.ToString());
            }
            SRecognize.SpeechRecognized += SRecognize_SpeechRecognized;
        }

        public static void LoadGrammar() {
            var keywords = new Choices();
            keywords.Add(Keywords.ToArray());
            keywords.Add(MusicList.GetAllSongKeywords());

            var grammerBuilder = new GrammarBuilder();
            grammerBuilder.Append(keywords);

            SRecognize.UnloadAllGrammars();
            SRecognize.LoadGrammar(new Grammar(grammerBuilder));
        }

        private static void SRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            var input = e.Result.Text;

            if (ListeningTimer.Instance.IsListening) {
                ExecuteCommand(input);
            }

            if (input == "music") {
                ListeningTimer.Instance.IncrementTime();
            }
        }

        public static void ExecuteCommand(string input) {
            SendMessage(input);
            try {
                if (SpeechCommands.ContainsKey(input)) {
                    SpeechCommands[input]();
                } else {
                    Player.Play(MusicList.GetMatchingSongs(input).Select(s => s.FilePath));
                    ListeningTimer.Instance.StopListening();
                }
            } catch (Exception e) {
                SendMessage(e.ToString());
            }
        }

        private static void SendMessage(string message) {
            if (MessageSend != null) {
                MessageSend(message);
            }
        }
    }
}