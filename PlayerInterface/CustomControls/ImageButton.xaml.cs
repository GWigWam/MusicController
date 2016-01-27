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

namespace PlayerInterface.CustomControls {

    /// <summary>
    /// Interaction logic for ImageButton.xaml
    /// </summary>
    public partial class ImageButton : UserControl, ICommandSource {

        public ImageSource Image {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));

        public bool GlowOnHover {
            get { return (bool)GetValue(GlowOnHoverProperty); }
            set { SetValue(GlowOnHoverProperty, value); }
        }

        public static readonly DependencyProperty GlowOnHoverProperty =
            DependencyProperty.Register("GlowOnHover", typeof(bool), typeof(ImageButton), new PropertyMetadata(null));

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ImageButton), new UIPropertyMetadata(null));

        public object CommandParameter {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ImageButton), new UIPropertyMetadata(null));

        public IInputElement CommandTarget {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(ImageButton), new UIPropertyMetadata(null));

        private Brush DefaultBack;
        private Brush HoverEnabledBack;
        private Brush HoverDisabledBack;

        public ImageButton() {
            GlowOnHover = true;
            InitializeComponent();

            DefaultBack = new SolidColorBrush(Colors.Transparent);
            HoverEnabledBack = new RadialGradientBrush(Color.FromArgb(0xFF, 0x34, 0x98, 0xDB), Color.FromArgb(0x00, 0x34, 0x98, 0xDB)) {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5,
                MappingMode = BrushMappingMode.RelativeToBoundingBox,
                SpreadMethod = GradientSpreadMethod.Pad
            };

            HoverDisabledBack = new RadialGradientBrush(Color.FromArgb(0xFF, 0xC0, 0x39, 0x2b), Color.FromArgb(0x00, 0xC0, 0x39, 0x2b)) {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5,
                MappingMode = BrushMappingMode.RelativeToBoundingBox,
                SpreadMethod = GradientSpreadMethod.Pad
            };

            ButtonImage.IsMouseDirectlyOverChanged += (s, a) => UpdateBackground();
        }

        private void UpdateBackground() {
            if(ButtonImage.IsMouseDirectlyOver) {
                if(Command?.CanExecute(null) ?? false) {
                    ContainerGrid.Background = HoverEnabledBack;
                } else {
                    ContainerGrid.Background = HoverDisabledBack;
                }
            } else {
                ContainerGrid.Background = DefaultBack;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);

            var command = Command;
            var parameter = CommandParameter;
            var target = CommandTarget;

            var routedCmd = command as RoutedCommand;
            if(routedCmd != null && routedCmd.CanExecute(parameter, target)) {
                routedCmd.Execute(parameter, target);
            } else if(command != null && command.CanExecute(parameter)) {
                command.Execute(parameter);
            }
        }
    }
}