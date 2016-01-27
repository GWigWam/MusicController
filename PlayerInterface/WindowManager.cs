using Hardcodet.Wpf.TaskbarNotification;
using PlayerCore;
using PlayerInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface {

    internal class WindowManager {

        private Application App {
            get;
        }

        private TaskbarIcon TrayIcon;
        private FullPlayer Full;
        private SmallPlayer Small;

        private SongPlayer SongPlayer;
        private Playlist Playlist;

        public WindowManager(Application app) {
            App = app;
        }

        public void Init(SongPlayer sp, Playlist pl) {
            SongPlayer = sp;
            Playlist = pl;

            TrayIcon = (TaskbarIcon)App.FindResource("Tbi_Icon");
            CreateSmallPlayer(true);
            CreateFullPlayer(false);
            TrayIcon.TrayMouseDoubleClick += (s, a) => ShowFullWindow();
            TrayIcon.TrayMouseMove += Tbi_TrayMouseMove;
        }

        private void CreateSmallPlayer(bool show) {
            var spvm = new SmallPlayerViewModel(SongPlayer, Playlist);
            Small = new SmallPlayer(spvm);
            Small.Btn_ShowFull.MouseLeftButtonUp += (s, a) => ShowFullWindow();
            if(show)
                Small.Show();
        }

        private void CreateFullPlayer(bool show) {
            var fpvm = new FullPlayerViewModel(SongPlayer, Playlist);
            Full = new FullPlayer(fpvm);
            Full.MinimizedToTray += (s, a) => ShowSmallWindow();
            if(show)
                Full.Show();
        }

        private void Tbi_TrayMouseMove(object sender, RoutedEventArgs e) {
            if(!(Full?.IsVisible ?? false)) {
                ShowSmallWindow();
            }
        }

        private void ShowSmallWindow() {
            Small.Show();
            Small.Activate();
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