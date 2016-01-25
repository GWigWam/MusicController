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
    /// Interaction logic for FullPlayer.xaml
    /// </summary>
    public partial class FullPlayer : Window {

        public event EventHandler MinimizedToTray;

        private bool SlidingElapsed = false;

        private FullPlayerViewModel Model => DataContext as FullPlayerViewModel;

        public FullPlayer(FullPlayerViewModel fpvm) {
            InitializeComponent();

            DataContext = fpvm;
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e) {
            if(e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Btn_Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Application.Current.Shutdown();
        }

        private void Btn_Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            MinimizeToTray();
        }

        public void MinimizeToTray() {
            Hide();
            MinimizedToTray?.Invoke(this, new EventArgs());
        }

        private void Slr_Elapsed_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if(e.LeftButton == MouseButtonState.Pressed) {
                if(Model.SongPlayer.PlayerState == NAudio.Wave.PlaybackState.Playing) {
                    Model.SongPlayer.PlayerState = NAudio.Wave.PlaybackState.Paused;
                    SlidingElapsed = true;
                }
            }
        }

        private void Slr_Elapsed_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            if(SlidingElapsed) {
                Model.SongPlayer.PlayerState = NAudio.Wave.PlaybackState.Playing;
                SlidingElapsed = false;
            }
        }
    }
}