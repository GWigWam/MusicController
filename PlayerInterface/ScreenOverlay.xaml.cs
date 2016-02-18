using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface {

    /// <summary>
    /// Interaction logic for ScreenOverlay.xaml
    /// </summary>
    public partial class ScreenOverlay : Window {
        private ScreenOverlayViewModel ViewModel => DataContext as ScreenOverlayViewModel;

        public string Text {
            get { return ViewModel.Text; }
            set {
                ViewModel.Text = value;
                LastActivity = Environment.TickCount;
                if(!IsVisible) {
                    Application.Current.Dispatcher.Invoke((() => Show()));
                }
            }
        }

        public int AutoHideTimeMs {
            get; set;
        }

        private long LastActivity;

        public ScreenOverlay(int autoHideTimeMs) {
            InitializeComponent();
            AutoHideTimeMs = autoHideTimeMs;

            Width = SystemParameters.WorkArea.Width;
            DataContext = new ScreenOverlayViewModel();

            new Timer() {
                AutoReset = true,
                Enabled = true,
                Interval = 99
            }.Elapsed += (s, a) => {
                var dif = Environment.TickCount - LastActivity;
                if(dif > AutoHideTimeMs && IsVisible) {
                    Application.Current.Dispatcher.Invoke((() => Hide()));
                }
            };
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e) {
            Hide();
        }
    }

    internal class ScreenOverlayViewModel : INotifyPropertyChanged {
        private string text;

        public string Text {
            get { return text; }
            set {
                if(value != text) {
                    text = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}