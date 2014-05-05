﻿using System;
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

        MusicList musicList = new MusicList(Settings.readMusicLocation());
        Player player = new Player(Settings.readAIMP3Location());

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

                System.Media.SystemSounds.Beep.Play();
            }
        }

        public void start(){
            Choices sList = new Choices();
            sList.Add(new string[] {"music", "switch", "random"});
            sList.Add(musicList.getAllSongNames().ToArray<string>());
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(sList);
            Grammar gr = new Grammar(gb);
            sRecognize.LoadGrammar(gr);

            sRecognize.SetInputToDefaultAudioDevice();
            sRecognize.RecognizeAsync(RecognizeMode.Multiple);
            sRecognize.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sRecognize_SpeechRecognized);
        }

        private void sRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            string result = e.Result.Text.ToString();

            if(result.Equals("music") && musicOn == false) {
                MusicOn = true;
                timer = new Timer(10000);
                timer.Elapsed += new ElapsedEventHandler(timerElapsed);
                timer.Enabled = true;
                return;
            }else {
                Console.WriteLine(result);
            }

            if(MusicOn) {
                try {
                    if(result.Equals("switch")) {
                        player.Toggle();
                        setMusicOff();
                        return;
                    } else if(result.Equals("random")) {
                        result = musicList.getRandomSongName();
                        player.Play("\"" + musicList.getSongLocation(result) + "\"");
                        return;
                    }else{
                        player.Play("\"" + musicList.getSongLocation(result) + "\""); //Add " at begin and end so AIMP3 gets it
                        setMusicOff();
                    }
                }catch(Exception e1){
                    Console.WriteLine(e1.Message);
                }
            }
        }

        private void timerElapsed(object sender, ElapsedEventArgs e) {
            setMusicOff();
        }

        private void setMusicOff() {
            MusicOn = false;
            timer.Enabled = false;
        }

        public void stop(){
            sRecognize.RecognizeAsyncStop();
        }
    }
}
