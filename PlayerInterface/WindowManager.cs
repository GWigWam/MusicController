using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerCore.Settings;
using PlayerInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface {

    internal class WindowManager {

        private App App {
            get;
        }

        private FullPlayerViewModel ViewModel;

        private TaskbarIcon TrayIcon;
        private FullPlayer Full;
        private SmallPlayer Small;

        public WindowManager(App app) {
            App = app;
        }

        public void Init(AppSettings settings, SongPlayer songPlayer, Playlist playlist) {
            TrayIcon = (TaskbarIcon)App.FindResource("Tbi_Icon");

            ViewModel = new FullPlayerViewModel(settings, songPlayer, playlist);

            CreateFullPlayer(!settings.StartMinimized);
            CreateSmallPlayer(settings.StartMinimized);

            SetupContextMenu();

            TrayIcon.TrayMouseDoubleClick += (s, a) => ShowFullWindow();
            TrayIcon.TrayLeftMouseUp += (s, a) => {
                if(!(Full?.IsVisible ?? false)) {
                    ShowSmallWindow();
                }
            };
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

        private void SetupContextMenu() {
            TrayIcon.ContextMenu.DataContext = new {
                SmallPlayer = new RelayCommand((o) => ShowSmallWindow()),
                FullPlayer = new RelayCommand((o) => ShowFullWindow()),
                Quit = new RelayCommand((o) => Application.Current.Shutdown())
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