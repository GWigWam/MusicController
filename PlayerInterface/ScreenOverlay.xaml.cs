using PlayerCore.Persist;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface {

    public partial class ScreenOverlay : Window {
        private ScreenOverlayViewModel ViewModel => DataContext as ScreenOverlayViewModel;

        private AppSettings Settings {
            get;
        }

        private long HideTimeStamp;

        private bool MouseInFormSinceStartup {
            get; set;
        }

        private long FormStartShowTime {
            get; set;
        }

        public ScreenOverlay(AppSettings settings) {
            InitializeComponent();
            Settings = settings;

            SetupSize();
            DataContext = new ScreenOverlayViewModel();
        }

        public void DisplayText(string text) {
            var time = (int)Settings.ScreenOverlayShowTimeMs;
            if(time > 0) {
                DisplayText(text, time);
            }
        }

        public void DisplayText(string text, TimeSpan autoHideTime) {
            DisplayText(text, (int)autoHideTime.TotalMilliseconds);
        }

        public void DisplayText(string text, int autoHideTimeMs) {
            Application.Current.Dispatcher.Invoke(() => {
                ViewModel.Text = text;
                if(!IsVisible) {
                    Show();
                }

                HideTimeStamp = Environment.TickCount + autoHideTimeMs;
                FormStartShowTime = Environment.TickCount;
                MouseInFormSinceStartup = false;
            });

            Task.Delay(autoHideTimeMs + 50).GetAwaiter().OnCompleted(() => {
                if(Environment.TickCount > HideTimeStamp && IsVisible) {
                    Application.Current.Dispatcher.Invoke((() => Hide()));
                }
            });
        }

        private void SetupSize() {
            Width = SystemParameters.WorkArea.Width;
            Left = 0;
            Top = 0;
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e) {
            if(!MouseInFormSinceStartup) {
                if(Environment.TickCount - FormStartShowTime < 50) {
                    MouseInFormSinceStartup = true;
                } else {
                    Hide();
                }
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if((bool)e.NewValue == true) { // Visible == true
                SetupSize();
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e) {
            MouseInFormSinceStartup = false;
        }
    }

    internal class ScreenOverlayViewModel : NotifyPropertyChanged {
        private string text;

        public string Text {
            get { return text; }
            set {
                if(value != text) {
                    text = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}