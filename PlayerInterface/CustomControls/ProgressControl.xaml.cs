using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayerInterface.CustomControls {

    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl, INotifyPropertyChanged {
        private const double ScrollChange = 0.05;

        public event PropertyChangedEventHandler PropertyChanged;

        public Brush ProgressFill {
            get { return (Brush)GetValue(ProgressFillProperty); }
            set { SetValue(ProgressFillProperty, value); }
        }

        public static readonly DependencyProperty ProgressFillProperty =
            DependencyProperty.Register(nameof(ProgressFill), typeof(Brush), typeof(ProgressControl), new PropertyMetadata(Brushes.White));

        public double Fraction {
            get { return (double)GetValue(FractionProperty); }
            set { SetValue(FractionProperty, value); }
        }

        public static readonly DependencyProperty FractionProperty =
            DependencyProperty.Register(nameof(Fraction), typeof(double), typeof(ProgressControl), new PropertyMetadata(0.0, new PropertyChangedCallback(OnFractionChanged)));

        public double ProgressWidth {
            get {
                var res = (ProgressControlCanvas?.ActualWidth ?? 0) * ((double)GetValue(FractionProperty));
                return res;
            }
        }

        public bool Editable {
            get { return (bool)GetValue(EditableProperty); }
            set { SetValue(EditableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Editable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditableProperty =
            DependencyProperty.Register("Editable", typeof(bool), typeof(ProgressControl), new PropertyMetadata(true));

        public ProgressControl() {
            InitializeComponent();
        }

        private static void OnFractionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            (sender as ProgressControl)?.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ProgressWidth)));
        }

        private void ProgressControlCanvas_MouseWheel(object sender, MouseWheelEventArgs e) {
            if(Editable) {
                var change = ((e.Delta < 0) ? -1 : 1) * ScrollChange;
                var newVal = Fraction + change;

                Fraction = newVal < 0 ? 0 : (newVal > 1 ? 1 : newVal);
            }
        }

        private void MouseHandler(object sender, MouseEventArgs e) {
            if(e.LeftButton == MouseButtonState.Pressed && Editable) {
                Point clickPos = e.GetPosition(ProgressControlCanvas);
                var frac = clickPos.X / ProgressControlCanvas.ActualWidth;
                Fraction = frac;
            }
        }
    }
}