using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Timers;

namespace SpeechMusicController {
    class SpeechInput {

        SpeechRecognitionEngine sRecognize = new SpeechRecognitionEngine();
        private Timer timer;
        MusicList musicList = new MusicList(@"F:\zooi\overige\muziek\");

        private bool musicOn = false;
        private bool MusicOn {
            get { return musicOn; }
            set {
                musicOn = value;
                if(musicOn == true) {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Music detect ON");
                } else {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine("Music detect OFF");
                }
            }
        }

        public void start(){
            Choices sList = new Choices();
            sList.Add(new string[] {"music"});
            sList.Add(musicList.getAllSongNames().ToArray<string>());
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(sList);
            Grammar gr = new Grammar(gb);
            //sRecognize.RecognizeAsyncStop();
            //sRecognize.RequestRecognizerUpdate();
            sRecognize.LoadGrammar(gr);

            //sRecognize.SpeechRecognized += sRecognize_SpeechRecognized;
            sRecognize.SetInputToDefaultAudioDevice();
            sRecognize.RecognizeAsync(RecognizeMode.Multiple);
            //sRecognize.Recognize();
            sRecognize.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sRecognize_SpeechRecognized);
        }

        private void sRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            string result = e.Result.Text.ToString();

            if(result.Equals("music")) {
                MusicOn = true;
                timer = new Timer(10000);
                timer.Elapsed += new ElapsedEventHandler(setMusicOnFalse);
                timer.Enabled = true;
            }else {
                Console.WriteLine(result);
            }

            if(MusicOn) { 
                
            }
            //Player p = new Player(@"F:\Program Files (x86)\AIMP3\AIMP3.exe");
            //p.play("\"F:\\zooi\\overige\\Muziek\\Muse - Uprising.mp3\"");
        }

        private void setMusicOnFalse(object sender, ElapsedEventArgs e) {
            MusicOn = false;
            timer.Enabled = false;
        }

        public void stop(){
            sRecognize.RecognizeAsyncStop();
        }
    }
}
