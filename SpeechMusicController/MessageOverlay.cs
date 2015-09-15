using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeechMusicController {

    public partial class MessageOverlay : Form {
        public const int DefaultShowMessageTimeMs = 500;

        //Prevent multiple overlays at the same time
        private static MessageOverlay CurrentOverlay;

        public MessageOverlay(string Message, int ShowMessageTimeMs = DefaultShowMessageTimeMs) {
            InitializeComponent();

            if(CurrentOverlay != null) {
                CurrentOverlay.Close();
            }
            CurrentOverlay = this;

            T_AutoClose.Interval = ShowMessageTimeMs;

            Screen screen = Screen.AllScreens[0];
            Left = 0;
            Top = 0;
            Width = screen.WorkingArea.Width;

            Lb_Message.Text = Message;
        }

        private void MessageOverlay_Load(object sender, EventArgs e) {
            T_AutoClose.Start();
        }

        private void T_AutoClose_Tick(object sender, EventArgs e) => CloseMessage();

        public void CloseMessage() {
            T_AutoClose.Stop();
            CurrentOverlay = null;
            this.Close();
        }
    }
}