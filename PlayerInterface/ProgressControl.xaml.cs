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

namespace PlayerInterface {

    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl, INotifyPropertyChanged {

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

        // Using a DependencyProperty as the backing store for Percentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FractionProperty =
            DependencyProperty.Register(nameof(Fraction), typeof(double), typeof(ProgressControl), new PropertyMetadata(0.0, new PropertyChangedCallback(OnFractionChanged)));

        public double ProgressWidth {
            get {
                var res = (ProgressControlCanvas?.ActualWidth ?? 0) * ((double)GetValue(FractionProperty));
                return res;
            }
        }

        public ProgressControl() {
            InitializeComponent();
        }

        private static void OnFractionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            (sender as ProgressControl)?.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ProgressWidth)));
        }
    }
}