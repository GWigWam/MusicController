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
            SetupTrayEvents();
        }

        private void Init() {
            TrayIcon = (TaskbarIcon)FindResource("Tbi_Icon");
            CreateSmallPlayer();
        }

        private void CreateSmallPlayer() {
            Small = new SmallPlayer();
            Small.Btn_ShowFull.MouseLeftButtonUp += (s, a) => ShowFullWindow();
            Small.Show();
        }

        private void CreateFullPlayer() {
            Full = new FullPlayer();
            Full.Closed += (s, a) => Full = null;
        }

        private void SetupTrayEvents() {
            TrayIcon.TrayMouseDoubleClick += (s, a) => ShowFullWindow();
            TrayIcon.TrayMouseMove += Tbi_TrayMouseMove;
        }

        private void Tbi_TrayMouseMove(object sender, RoutedEventArgs e) {
            if((!(Full?.IsVisible ?? false))) {
                Small.WindowState = WindowState.Normal;
                Small.Activate();
            }
        }

        private void ShowFullWindow() {
            if(Full == null) {
                CreateFullPlayer();
            }
            if(!Full.IsVisible) {
                Full.Show();
            }
            Full.Activate();
            Small.WindowState = WindowState.Minimized;
        }
    }
}