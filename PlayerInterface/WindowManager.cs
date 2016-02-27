using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Settings;
using PlayerInterface.Commands;
using PlayerInterface.ViewModels;
using SpeechControl;
using System;
using System.Linq;
using System.Windows;

namespace PlayerInterface {

    internal class WindowManager {
        private FullPlayerViewModel ViewModel;

        public TaskbarIcon TrayIcon { get; protected set; }
        public FullPlayer Full { get; protected set; }
        public SmallPlayer Small { get; protected set; }
        public ScreenOverlay Overlay { get; protected set; }

        public WindowManager(TaskbarIcon icon) {
            TrayIcon = icon;
        }

        public void Init(AppSettings settings, SongPlayer songPlayer, Playlist playlist, SpeechController speechControl) {
            ViewModel = new FullPlayerViewModel(settings, songPlayer, playlist, speechControl);

            CreateFullPlayer(!settings.StartMinimized);
            CreateSmallPlayer(settings.StartMinimized);

            SetupContextMenu(songPlayer);

            TrayIcon.TrayMouseDoubleClick += (s, a) => ShowFullWindow();
            TrayIcon.TrayLeftMouseUp += (s, a) => {
                if(!(Full?.IsVisible ?? false)) {
                    ShowSmallWindow();
                }
            };

            SetupScreenOverlay(settings, speechControl, songPlayer);
        }

        private void CreateSmallPlayer(bool show) {
            Small = new SmallPlayer(ViewModel);
            Small.Btn_ShowFull.Command = new RelayCommand((o) => ShowFullWindow());

            if(show)
                Small.Show();
        }

        private void CreateFullPlayer(bool show) {
            Full = new FullPlayer(ViewModel);
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
        }

        private void SetupScreenOverlay(AppSettings settings, SpeechController speech, SongPlayer player) {
            Overlay = new ScreenOverlay(settings);

            speech.PartialSentenceMatch += (s, a) => {
                Application.Current.Dispatcher.Invoke(() => Overlay.DisplayText(a.Sentence.Aggregate((acc, cur) => $"{acc} '{cur}'")));
            };

            speech.FullSentenceMatch += (s, a) => {
                Application.Current.Dispatcher.Invoke(() => Overlay.DisplayText($"- {a.Sentence.Aggregate((acc, cur) => $"{acc} {cur}")} -"));
            };

            player.SongChanged += (s, a) => {
                if(player?.CurrentSong != null) {
                    Application.Current.Dispatcher.Invoke(() => Overlay.DisplayText($"{player.CurrentSong.Title} - {player.CurrentSong.Artist}"));
                }
            };
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