using SpeechMusicController.AppSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;

namespace SpeechMusicController {

    internal class SpeechInput {
        private Settings AppSettings;
        private CommandModeTimer ModeTimer;
        private MusicList MusicCollection;
        private IPlayer Player;
        private Random RNG;
        private IReadOnlyList<SpeechCommand> SpeechCommands;

        private SpeechRecognitionEngine SRecognize;

        public SpeechInput(Settings settings, MusicList musicCollection, string playerPath) {
            ModeTimer = new CommandModeTimer();
            RNG = new Random();
            AppSettings = settings;
            SRecognize = new SpeechRecognitionEngine();
            Player = new Aimp3Player(playerPath);
            if(musicCollection != null) {
                MusicCollection = musicCollection;
            } else {
                throw new ArgumentNullException(nameof(musicCollection));
            }
            InitCommands();

            try {
                LoadGrammar();

                SRecognize.SetInputToDefaultAudioDevice();
                SRecognize.RecognizeAsync(RecognizeMode.Multiple);
            } catch(Exception e) {
                System.Windows.Forms.MessageBox.Show("Error while starting SpeechInput\n" + e.ToString());
            }
            SRecognize.SpeechRecognized += SRecognize_SpeechRecognized;

            AppSettings.OnChange += (s, a) => LoadGrammar();
        }

        public event Action<string> MessageSend;

        public bool Listening {
            get; set;
        } = true;

        public IEnumerable<string> Keywords {
            get {
                if(ModeTimer != null) {
                    foreach(var modeName in CommandModeTimer.ModeNames) {
                        yield return modeName;
                    }
                }
                if(SpeechCommands != null) {
                    foreach(var command in SpeechCommands.Select(sc => sc.Keyword)) {
                        yield return command;
                    }
                }
            }
        }

        public void ExecuteCommand(string input, bool ignoreActiveMatchMode = false) {
            try {
                if(CommandModeTimer.ModeNames.Contains(input, StringComparer.InvariantCultureIgnoreCase)) {
                    long showTime;
                    long modeTime;
                    if(AppSettings.TryGetSetting("MessageOverlayVisibleTimeMs", out showTime) && AppSettings.TryGetSetting("CommandModeActiveTimeMs", out modeTime)) {
                        new MessageOverlay(input, (int)showTime).Show();
                        ModeTimer.ActivateMode(input, (int)modeTime);
                    }
                } else {
                    SpeechCommand matchingCommand = SpeechCommands.FirstOrDefault(sc => sc.Keyword.Equals(input, StringComparison.InvariantCultureIgnoreCase));
                    if(matchingCommand != null) {
                        if(ModeTimer.MusicModeActive || ignoreActiveMatchMode) {
                            matchingCommand.Procedure?.Invoke();
                        }
                    } else {
                        Song[] songs;
                        if(ModeTimer.MusicModeActive || ignoreActiveMatchMode) {
                            songs = MusicCollection.GetMatchingSongs(input);
                        } else {
                            songs = MusicCollection.GetMatchingSongs(input, ModeTimer.SongModeActive, ModeTimer.ArtistModeActive, ModeTimer.AlbumModeActive);
                        }
                        if(songs.Length > 0) {
                            Player.Play(songs.Select(s => s.FilePath));

                            ResetModes();
                        }
                    }
                }
            } catch(Exception e) {
                SendMessage(e.ToString());
            }
        }

        private void InitCommands() {
            SpeechCommands = new List<SpeechCommand>() {
                new SpeechCommand("collection", () => {
                    Player.Play(MusicCollection.ActiveSongs.OrderBy(s => RNG.Next()).Select(s => s.FilePath));
                    ResetModes();
                }),
                new SpeechCommand("full collection", () => {
                    Player.Play(MusicCollection.AllSongs.OrderBy(s => RNG.Next()).Select(s => s.FilePath));
                    ResetModes();
                }),
                new SpeechCommand("switch", () => {
                        Player.Toggle();
                        ResetModes();
                    }),
                new SpeechCommand("random", () => Player.Play(MusicCollection.GetRandomSong().FilePath)),
                new SpeechCommand("next", Player.Next),
                new SpeechCommand("previous", Player.Previous),
                new SpeechCommand("volume up", Player.VolUp),
                new SpeechCommand("volume down", Player.VolDown),
                new SpeechCommand("play", Player.Play)
            };
        }

        private void LoadGrammar() {
            var keywords = new Choices();
            keywords.Add(Keywords.ToArray());
            keywords.Add(MusicCollection.GetAllSongKeywords());

            var grammerBuilder = new GrammarBuilder();
            grammerBuilder.Append(keywords);

            SRecognize.UnloadAllGrammars();
            SRecognize.LoadGrammar(new Grammar(grammerBuilder));
        }

        private void ResetModes() {
            ModeTimer.ActivateMode(CommandMode.None, 0);
        }

        private void SendMessage(string mesage) => MessageSend?.Invoke(mesage ?? "");

        private void SRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            string status = Listening ? $"{e.Result.Confidence * 100:#}%" : "DISABLED";
            SendMessage($"{e.Result.Text} ({status})");
            if(Listening) {
                ExecuteCommand(e.Result.Text);
            }
        }
    }
}