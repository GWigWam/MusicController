using SpeechMusicController.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechMusicController {

    public class SpeechInput {
        public static string[] KEYWORDS = new string[] { "music", "switch", "random", "next", "previous", "collection", "volume up", "volume down" };
        private MainForm f1;
        private SpeechRecognitionEngine sRecognize = new SpeechRecognitionEngine();

        private Player player = new Player(PathSettings.ReadAIMP3Location());

        public SpeechInput(MainForm inForm1) {
            f1 = inForm1;
            SongRules.GetRules();
        }

        public void Start() {
            Choices sList = new Choices();
            sList.Add(KEYWORDS);
            sList.Add(MusicList.GetAllSongKeywords());
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(sList);
            Grammar gr = new Grammar(gb);
            sRecognize.LoadGrammar(gr);

            try {
                sRecognize.SetInputToDefaultAudioDevice();
                sRecognize.RecognizeAsync(RecognizeMode.Multiple);
                sRecognize.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sRecognize_SpeechRecognized);
            } catch { }
        }

        private void sRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            var input = e.Result.Text;

            if (ListeningTimer.Instance.IsListening) {
                ExecuteCommand(input);
            }

            if (input == "music") {
                ListeningTimer.Instance.IncrementTime();
            }
        }

        public void ExecuteCommand(string input) {
            f1.WriteLine(input);
            try {
                if (input == "switch") {
                    player.Toggle();
                    ListeningTimer.Instance.StopListening();
                } else if (input == "random") {
                    player.Play(MusicList.GetRandomSong());
                } else if (input == "next") {
                    player.Next();
                } else if (input == "previous") {
                    player.Previous();
                } else if (input == "collection") {
                    player.Play(MusicList.ActiveSongs.ToArray(), false);
                    ListeningTimer.Instance.StopListening();
                } else if (input == "volume up") {
                    player.VolUp();
                } else if (input == "volume down") {
                    player.VolDown();
                } else {
                    player.Play(MusicList.GetMatchingSongs(input));
                    ListeningTimer.Instance.StopListening();
                }
            } catch (Exception e1) {
                f1.WriteLine(e1.Message);
            }
        }
    }
}