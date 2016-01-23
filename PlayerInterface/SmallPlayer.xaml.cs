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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width;
            Top = workArea.Bottom - Height;
        }
    }
}