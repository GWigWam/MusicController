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

    public partial class ImageButton : UserControl, INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource Image {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ImageButton), new UIPropertyMetadata(null, (s, a) => ((ImageButton)s)?.CommandChanged()));

        public object CommandParameter {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ImageButton), new UIPropertyMetadata(null, (s, a) => ((ImageButton)s).UpdateCanCommandExecute()));
        
        public Brush ButtonColor {
            get { return (Brush)GetValue(ButtonColorProperty); }
            set { SetValue(ButtonColorProperty, value); }
        }
        
        public static readonly DependencyProperty ButtonColorProperty =
            DependencyProperty.Register(nameof(ButtonColor), typeof(Brush), typeof(ImageButton), new PropertyMetadata(Brushes.Red));

        private bool _canCommandExecute;
        public bool CanCommandExecute {
            get => _canCommandExecute;
            set {
                if (_canCommandExecute != value) {
                    _canCommandExecute = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCommandExecute)));
                }
            }
        }

        public ImageButton() {
            InitializeComponent();
        }

        private void ControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (Command != null && Command.CanExecute(CommandParameter)) {
                Command.Execute(CommandParameter);
            }
        }

        private void CommandChanged() {
            if (Command != null) {
                Command.CanExecuteChanged += (s, a) => UpdateCanCommandExecute();
            }
            UpdateCanCommandExecute();
        }

        private void UpdateCanCommandExecute() {
            Dispatcher.Invoke(() => CanCommandExecute = Command?.CanExecute(CommandParameter) ?? true);
        }
    }
}
