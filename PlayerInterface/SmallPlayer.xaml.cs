using PlayerInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlayerInterface {

    /// <summary>
    /// Interaction logic for SmallPlayer.xaml
    /// </summary>
    public partial class SmallPlayer : Window {

        public SmallPlayer(SmallPlayerViewModel spvm) {
            InitializeComponent();

            DataContext = spvm;
            spvm.SongPlayer.PlayingStopped += (s, a) => {
                if(a.Exception != null) {
                    Dispatcher.Invoke(() => {
                        new ExceptionWindow(a.Exception).Show();
                    });
                }
            };
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(IsVisible) {
                MoveToCorner();
            }
        }

        private void MoveToCorner() {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width;
            Top = workArea.Bottom - Height;
        }
    }
}