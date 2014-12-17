using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Speech.Recognition;

namespace SpeechMusicController {
    public class SpeechInput {
        public static string[] KEYWORDS = new string[] { "music", "switch", "random", "next", "previous", "collection", "volume up", "volume down" };
        Form1 f1;
        SpeechRecognitionEngine sRecognize = new SpeechRecognitionEngine();

        Player player = new Player(Settings.ReadAIMP3Location());

        public bool Enabled = true;
        private bool MusicOn;
        //private bool VolumeOn;

        public SpeechInput(Form1 inForm1) {
            f1 = inForm1;
        }

        public void Start() {
            Choices sList = new Choices();
            sList.Add(KEYWORDS);
            sList.Add(MusicList.GetAllSongAndBandNames().ToArray<string>());
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(sList);
            Grammar gr = new Grammar(gb);
            sRecognize.LoadGrammar(gr);
            
            sRecognize.SetInputToDefaultAudioDevice();
            sRecognize.RecognizeAsync(RecognizeMode.Multiple);
            sRecognize.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sRecognize_SpeechRecognized);
        }

        private void sRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            if (Enabled) {
                var input = e.Result.Text;
                if (MusicOn) {
                    ExecuteCommand(input);
                } else if (input == "music") {
                    MusicOn = true;
                    System.Media.SystemSounds.Beep.Play();
                    Task.Delay(10000).GetAwaiter().OnCompleted(() => {
                        if (MusicOn) {
                            MusicOn = false;
                            System.Media.SystemSounds.Beep.Play();
                        }
                    });
                }
                f1.WriteLine(input);
            }
        }

        public void ExecuteCommand(string input) {
            try {
                if (input == "switch") {
                    player.Toggle();
                    MusicOn = false;
                } else if (input == "random") {
                    player.Play(MusicList.GetRandomSong());
                } else if (input == "next") {
                    player.Next();
                } else if (input == "previous") {
                    player.Previous();
                } else if (input == "collection") {
                    player.Play(MusicList.SongList);
                    MusicOn = false;
                } else if (input == "volume up") {
                    player.VolUp();
                } else if (input == "volume down") {
                    player.VolDown();
                } else {
                    player.Play(MusicList.GetMatchingSongs(input));
                    MusicOn = false;
                }
            } catch (Exception e1) {
                f1.WriteLine(e1.Message);
            }
        }
    }
}
