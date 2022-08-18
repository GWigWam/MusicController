using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Scrobbling;
using PlayerCore.Settings;
using PlayerInterface.Commands;
using PlayerInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface {
    internal class WindowManager {
        public TaskbarIcon TrayIcon { get; protected set; }
        public FullPlayer Full { get; protected set; }
        public SmallPlayer Small { get; protected set; }
        public ScreenOverlay Overlay { get; protected set; }

        public Window CurrentWindow { get; private set; }

        public WindowManager(TaskbarIcon icon) {
            TrayIcon = icon;
        }

        public void Init(AppSettings settings, SongPlayer songPlayer, Playlist playlist, TransitionManager transitionMngr, Scrobbler scrobbler) {
            var playVm = new PlayingVm(songPlayer, transitionMngr);
            var npVm = new NextPrevVm(songPlayer, playlist);

            var smallVm = new SmallPlayerViewModel(playVm, npVm, settings);
            var fullVm = new FullPlayerViewModel(settings, songPlayer, playlist, playVm, npVm, scrobbler);

            CreateFullPlayer(!settings.StartMinimized, fullVm);
            CreateSmallPlayer(settings.StartMinimized, smallVm);
            CurrentWindow = settings.StartMinimized ? Window.Small : Window.Full;

            SetupContextMenu(songPlayer);
            SetupScreenOverlay(settings, songPlayer);
            HandleWindowStateChanges();
            AddMinMaxEvents();

            scrobbler.ExceptionOccured += ExceptionWindow.Show;
        }

        private void CreateSmallPlayer(bool show, SmallPlayerViewModel vm) {
            Small = new SmallPlayer(vm);
            Small.Btn_ShowFull.Command = new RelayCommand(ShowFullWindow);

            if (show) {
                Small.Show();
            }
        }

        private void CreateFullPlayer(bool show, FullPlayerViewModel vm) {
            Full = new FullPlayer(vm);
            Full.MinimizedToTray += (s, a) => ShowSmallWindow();
            if (show) {
                Full.Show();
            }
        }

        private void SetupContextMenu(SongPlayer player) {
            var tivm = new TrayIconViewModel() {
                SmallPlayer = new RelayCommand(ShowSmallWindow),
                FullPlayer = new RelayCommand(ShowFullWindow),
                Quit = new RelayCommand(Application.Current.Shutdown)
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

        private void SetupScreenOverlay(AppSettings settings, SongPlayer player) {
            Overlay = new ScreenOverlay(settings);

            player.SongChanged += (s, a) => {
                if(player?.CurrentSong != null && CurrentWindow != Window.Full) {
                    Application.Current.Dispatcher.Invoke(() => Overlay.DisplayText($"{player.CurrentSong.Title} - {player.CurrentSong.Artist}"));
                }
            };
        }

        private void HandleWindowStateChanges() {
            Small.StateChanged += (s, a) => {
                if (Small.WindowState == WindowState.Maximized) {
                    Small.WindowState = WindowState.Normal;
                }
            };

            Full.StateChanged += (s, a) => {
                if (Full.WindowState == WindowState.Minimized) {
                    Full.WindowState = WindowState.Normal;
                    ShowSmallWindow();
                }
            };
        }

        private void AddMinMaxEvents() {
            Small.PreviewKeyDown += (s, a) => {
                if ((a.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || a.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && a.Key == Key.Up) {
                    ShowFullWindow();
                }
            };
            Full.PreviewKeyDown += (s, a) => {
                if ((a.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || a.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && a.Key == Key.Down) {
                    ShowSmallWindow();
                }
            };
        }

        private void ShowSmallWindow() {
            Small.Show();
            Small.Activate();
            Full.Hide();

            CurrentWindow = Window.Small;
        }

        private void ShowFullWindow() {
            if(!Full.IsVisible) {
                Full.WindowState = WindowState.Normal;
            }
            Full.Show();
            Full.Activate();
            Small.Hide();

            CurrentWindow = Window.Full;
        }

        public enum Window {
            Small, Full
        }
    }
}