using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Timers;

namespace SpeechMusicController {
    class SpeechInput {

        Form1 f1;
        SpeechRecognitionEngine sRecognize = new SpeechRecognitionEngine();
        private Timer timer;

        MusicList musicList = new MusicList(Settings.ReadMusicLocation());
        Player player = new Player(Settings.ReadAIMP3Location());

        public bool Enabled = true;
        
        private bool musicOn = false;
        private bool MusicOn {
            get { return musicOn; }
            set {
                musicOn = value;
                if(musicOn == true) {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                } else {
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                System.Media.SystemSounds.Beep.Play();
            }
        }

        public SpeechInput(Form1 inForm1) {
            f1 = inForm1;
        }

        public void Start(){
            Choices sList = new Choices();
            sList.Add(new string[] {"music", "switch", "random", "next", "previous", "collection"});
            sList.Add(musicList.GetAllSongAndBandNames().ToArray<string>());
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(sList);
            Grammar gr = new Grammar(gb);
            sRecognize.LoadGrammar(gr);

            sRecognize.SetInputToDefaultAudioDevice();
            sRecognize.RecognizeAsync(RecognizeMode.Multiple);
            sRecognize.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sRecognize_SpeechRecognized);
        }

        private void sRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            if(Enabled) {
                string result = e.Result.Text.ToString();

                if(result.Equals("music") && musicOn == false) {
                    MusicOn = true;
                    timer = new Timer(10000);
                    timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
                    timer.Enabled = true;
                    return;
                } else {
                    f1.WriteLine(result);
                }

                if(MusicOn) {
                    try {
                        if(result.Equals("switch")) {
                            player.Toggle();
                            SetMusicOff();
                            return;
                        } else if(result.Equals("random")) {
                            player.Play(musicList.GetRandomSong());
                            return;
                        } else if(result.Equals("next")) {
                            player.Next();
                            return;
                        } else if(result.Equals("previous")) {
                            player.Previous();
                            return;
                        } else if(result.Equals("collection")) {
                            player.PlayAll();
                            SetMusicOff();
                        } else {
                            player.Play(musicList.GetMatchingSongs(result));
                            SetMusicOff();
                            return;
                        }
                    } catch(Exception e1) {
                        f1.WriteLine(e1.Message);
                    }
                }
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e) {
            SetMusicOff();
        }

        private void SetMusicOff() {
            MusicOn = false;
            timer.Enabled = false;
        }
    }
}
