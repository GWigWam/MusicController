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

        MusicList musicList = new MusicList(Settings.ReadMusicLocation());
        Player player = new Player(Settings.ReadAIMP3Location());

        public bool Enabled = true;
        private bool MusicOn { get; set; }

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
                var result = e.Result.Text.ToString();
                if(MusicOn) {
                    try {
                        if(result.Equals("switch")) {
                            player.Toggle();
                            MusicOn = false;
                        } else if(result.Equals("random")) {
                            player.Play(musicList.GetRandomSong());
                        } else if(result.Equals("next")) {
                            player.Next();
                        } else if(result.Equals("previous")) {
                            player.Previous();
                        } else if(result.Equals("collection")) {
                            player.Play(musicList.SongList);
                            MusicOn = false;
                        } else {
                            player.Play(musicList.GetMatchingSongs(result));
                            MusicOn = false;
                        }
                    } catch(Exception e1) {
                        f1.WriteLine(e1.Message);
                    }
                } else if (result.Equals("music")) {
                    MusicOn = true;
                    System.Media.SystemSounds.Beep.Play();
                    Task.Delay(10000).GetAwaiter().OnCompleted(() => {
                        if (MusicOn) {
                            MusicOn = false;
                            System.Media.SystemSounds.Beep.Play();
                        }
                    });
                } else {
                    f1.WriteLine(result);
                }
            }
        }
    }
}
