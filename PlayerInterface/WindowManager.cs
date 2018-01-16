using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Settings;
using PlayerInterface.Commands;
using PlayerInterface.ViewModels;
using SpeechControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PlayerInterface {
    internal class WindowManager {
        public TaskbarIcon TrayIcon { get; protected set; }
        public FullPlayer Full { get; protected set; }
        public SmallPlayer Small { get; protected set; }
        public ScreenOverlay Overlay { get; protected set; }

        public WindowManager(TaskbarIcon icon) {
            TrayIcon = icon;
        }

        public void Init(AppSettings settings, SongPlayer songPlayer, Playlist playlist, SpeechController speechControl, TransitionManager transitionMngr) {
            var playVm = new PlayingVm(songPlayer, transitionMngr);
            var npVm = new NextPrevVm(songPlayer, playlist);
            var smallVm = new SmallPlayerViewModel(playVm, npVm);
            var fullVm = new FullPlayerViewModel(settings, songPlayer, playlist, speechControl, transitionMngr, playVm, npVm);

            CreateFullPlayer(!settings.StartMinimized, fullVm);
            CreateSmallPlayer(settings.StartMinimized, smallVm);

            SetupContextMenu(songPlayer);

            SetupScreenOverlay(settings, speechControl, songPlayer);
        }

        private void CreateSmallPlayer(bool show, SmallPlayerViewModel vm) {
            Small = new SmallPlayer(vm);
            Small.Btn_ShowFull.Command = new RelayCommand((o) => ShowFullWindow());

            if(show)
                Small.Show();
        }

        private void CreateFullPlayer(bool show, FullPlayerViewModel vm) {
            Full = new FullPlayer(vm);
            Full.MinimizedToTray += (s, a) => ShowSmallWindow();
            if(show)
                Full.Show();
        }

        private void SetupContextMenu(SongPlayer player) {
            var tivm = new TrayIconViewModel() {
                SmallPlayer = new RelayCommand((o) => ShowSmallWindow()),
                FullPlayer = new RelayCommand((o) => ShowFullWindow()),
                Quit = new RelayCommand((o) => Application.Current.Shutdown())
            };

            player.SongChanged += (s, a) => {
                tivm.ToolTipText = player?.CurrentSong?.Title;
            };

            TrayIcon.DataContext = tivm;

            TrayIcon.TrayMouseDoubleClick += (s, a) => ShowFullWindow();
            TrayIcon.TrayLeftMouseUp += (s, a) => {
                if(!(Full?.IsVisible ?? false)) {
                    ShowSmallWindow();
                } else if(Full?.IsVisible ?? false) {
                    ShowFullWindow();
                }
            };
        }

        private void SetupScreenOverlay(AppSettings settings, SpeechController speech, SongPlayer player) {
            Overlay = new ScreenOverlay(settings);

            speech.PartialSentenceMatch += (s, a) => {
                if(settings.EnableSpeech) {
                    Application.Current.Dispatcher.Invoke(() => Overlay.DisplayText(a.Sentence.Aggregate((acc, cur) => $"{acc} '{cur}'")));
                }
            };

            speech.FullSentenceMatch += (s, a) => {
                if(settings.EnableSpeech) {
                    Application.Current.Dispatcher.Invoke(() => Overlay.DisplayText($"- {a.Sentence.Aggregate((acc, cur) => $"{acc} {cur}")} -"));
                }
            };

            player.SongChanged += (s, a) => {
                if(player?.CurrentSong != null) {
                    Application.Current.Dispatcher.Invoke(() => Overlay.DisplayText($"{player.CurrentSong.Title} - {player.CurrentSong.Artist}"));
                }
            };

            speech.Commands.Add(new SpeechCommand() {
                KeyWords = new List<IEnumerable<string>>() { new string[] { "current song" } },
                Description = "Current song : Display current song name",
                Execute = (sentence) => {
                    var s = player.CurrentSong;
                    Overlay.DisplayText($"{s.Title} - {s.Artist} ({s.Album})", 5000);
                    return new string[0];
                },
                CanExecute = () => speech.Settings.EnableSpeech && player.CurrentSong != null
            });
        }

        private void ShowSmallWindow() {
            Small.Show();
            Small.Activate();
            Full.Hide();
        }

        private void ShowFullWindow() {
            if(!Full.IsVisible) {
                Full.WindowState = WindowState.Normal;
            }
            Full.Show();
            Full.Activate();
            Small.Hide();
        }
    }
}