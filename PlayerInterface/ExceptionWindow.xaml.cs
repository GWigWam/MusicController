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
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window {

        public ExceptionWindow(Exception e) {
            InitializeComponent();
            try {
                Tb_Content.Text = ExceptionToString(e);
            } catch {
                //Don't cause any further trouble
                Close();
            }
        }

        private string ExceptionToString(Exception e) {
            string inner = string.Empty;
            if(e.InnerException != null) {
                inner = $"Inner Exception:\n{ExceptionToString(e.InnerException)}";
            }
            return $"-- {e.GetType().FullName} --\n{e.Message}\n\n{e.StackTrace}\n\n{inner}";
        }
    }
}