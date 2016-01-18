using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface {

    public partial class App : Application {
        private TaskbarIcon TrayIcon;
        private FullPlayer Full;
        private SmallPlayer Small;

        private void Application_Startup(object sender, StartupEventArgs e) {
            Init();
            TrayIcon.TrayMouseDoubleClick += (s, a) => ShowFullWindow();
            TrayIcon.TrayMouseMove += Tbi_TrayMouseMove;
        }

        private void Init() {
            TrayIcon = (TaskbarIcon)FindResource("Tbi_Icon");
            CreateSmallPlayer(true);
            CreateFullPlayer(false);
        }

        private void CreateSmallPlayer(bool show) {
            Small = new SmallPlayer();
            Small.Btn_ShowFull.MouseLeftButtonUp += (s, a) => ShowFullWindow();
            if(show)
                Small.Show();
        }

        private void CreateFullPlayer(bool show) {
            Full = new FullPlayer();
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