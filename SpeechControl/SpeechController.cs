﻿using PlayerCore;
using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace SpeechControl {

    public class SpeechController {

        public SongPlayer Player {
            get;
        }

        public Playlist Playlist {
            get;
        }

        public AppSettings Settings {
            get;
        }

        public IEnumerable<SpeechCommand> Commands {
            get; private set;
        }

        public event EventHandler<SentenceChangedEventArgs> SentenceChanged;

        private SpeechRecognitionEngine SRecognize {
            get; set;
        }

        private long LastInputTime;

        private List<string> Sentence {
            get; set;
        }

        public SpeechController(SongPlayer player, Playlist playlist, AppSettings settings) {
            Player = player;
            Playlist = playlist;
            Settings = settings;

            LastInputTime = Environment.TickCount;
            Sentence = new List<string>();
        }

        public void Init() {
            try {
                SRecognize = new SpeechRecognitionEngine();
                Commands = SpeechCommand.CreateCommands(this);
                LoadGrammar();

                SRecognize.SetInputToDefaultAudioDevice();
                SRecognize.RecognizeAsync(RecognizeMode.Multiple);
            } catch(Exception) {
                //Todo: throw userfriendly exception
            }

            SRecognize.SpeechRecognized += SRecognize_SpeechRecognized;
            Playlist.ListContentChanged += (s, a) => LoadGrammar();
        }

        private void LoadGrammar() {
            var keywords = new Choices();
            keywords.Add(Commands.GetAllKeywords().ToArray());

            var grammerBuilder = new GrammarBuilder();
            grammerBuilder.Append(keywords);

            SRecognize.UnloadAllGrammars();
            SRecognize.LoadGrammar(new Grammar(grammerBuilder));
        }

        private void SRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            if(Environment.TickCount - LastInputTime > Settings.ResetSentenceTimeMs) {
                Sentence.Clear();
            }
            LastInputTime = Environment.TickCount;

            var add = e.Result.Text;
            Sentence.Add(add);
            SentenceChanged?.Invoke(this, new SentenceChangedEventArgs(Sentence, add));

            for(int skip = Sentence.Count - 1; skip >= 0; skip--) {
                var match = Commands.FirstOrDefault(sc => sc.IsMatch(Sentence.Skip(skip)));
                if(match != null) {
                    var retSentence = match.ExecuteCommand(Sentence.Skip(skip));
                    Sentence.Clear();
                    Sentence.AddRange(retSentence);
                    break;
                }
            }
        }
    }

    public class SentenceChangedEventArgs : EventArgs {

        public IEnumerable<string> Sentence {
            get;
        }

        public string Change {
            get;
        }

        public SentenceChangedEventArgs(IEnumerable<string> sentence, string change) : base() {
            Sentence = sentence;
            Change = change;
        }
    }
}