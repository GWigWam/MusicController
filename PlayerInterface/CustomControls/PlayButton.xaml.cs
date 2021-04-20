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
using System.Windows.Navigation;
using System.Windows.Shapes;

#nullable enable
namespace PlayerInterface.CustomControls
{
    public partial class PlayButton : UserControl
    {
        public PlayingVm? PlayingVm {
            get => (PlayingVm?)GetValue(PlayingVmProperty);
            set => SetValue(PlayingVmProperty, value);
        }
        public static readonly DependencyProperty PlayingVmProperty = DependencyProperty.Register(nameof(PlayingVm), typeof(PlayingVm), typeof(PlayButton), new PropertyMetadata(null));

        public PlayButton()
        {
            InitializeComponent();
        }

        private void Btn_Switch_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Popup.IsOpen = !Popup.IsOpen;
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = false;
        }
    }
}
