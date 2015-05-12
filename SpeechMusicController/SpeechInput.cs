using SpeechMusicController.AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechMusicController {
    public static class SpeechInput {
        public static string[] Keywords = new string[] { "music", "switch", "random", "next", "previous", "collection", "volume up", "volume down" };

        public static event Action<string> MessageSend;

        private static SpeechRecognitionEngine SRecognize = new SpeechRecognitionEngine();

        private static IPlayer Player;

        static SpeechInput() {
            string path = Settings.Instance.GetSetting("PlayerPath");
            if (!string.IsNullOrEmpty(path)) {
                Player = new Aimp3Player(path);
                Start();

                Settings.Instance.OnRulesChanged += LoadGrammar;
            } else {
                System.Windows.Forms.MessageBox.Show("Error: PlayerPath setting is empty");
            }
        }

        private static void Start() {
            LoadGrammar();

            SRecognize.SetInputToDefaultAudioDevice();
            SRecognize.RecognizeAsync(RecognizeMode.Multiple);
            SRecognize.SpeechRecognized += SRecognize_SpeechRecognized;
        }

        public static void LoadGrammar() {
            Choices sList = new Choices();
            sList.Add(Keywords);
            sList.Add(MusicList.GetAllSongKeywords());
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(sList);
            Grammar gr = new Grammar(gb);
            SRecognize.UnloadAllGrammars();
            SRecognize.LoadGrammar(gr);
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
                if (input == "switch") {
                    Player.Toggle();
                    ListeningTimer.Instance.StopListening();
                } else if (input == "random") {
                    Player.Play(MusicList.GetRandomSong());
                } else if (input == "next") {
                    Player.Next();
                } else if (input == "previous") {
                    Player.Previous();
                } else if (input == "collection") {
                    Random rand = new Random();
                    Player.Play(MusicList.ActiveSongs.OrderBy(s => rand.Next()).ToArray());
                    ListeningTimer.Instance.StopListening();
                } else if (input == "volume up") {
                    Player.VolUp();
                } else if (input == "volume down") {
                    Player.VolDown();
                } else {
                    Player.Play(MusicList.GetMatchingSongs(input));
                    ListeningTimer.Instance.StopListening();
                }
            } catch (Exception e1) {
                SendMessage(e1.Message);
            }
        }

        private static void SendMessage(string message) {
            if (MessageSend != null) {
                MessageSend(message);
            }
        }
    }
}