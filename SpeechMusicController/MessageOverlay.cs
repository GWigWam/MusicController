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

        public bool HideOnHover {
            get;
        }

        //Prevent multiple overlays at the same time
        private static MessageOverlay CurrentOverlay;

        private bool MouseInBoundsSinceStart;

        public MessageOverlay(string Message, int ShowMessageTimeMs, bool hideOnHover = true) {
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

            HideOnHover = hideOnHover;
            if(HideOnHover) {
                MouseInBoundsSinceStart = ClientRectangle.Contains(PointToClient(MousePosition));
                Lb_Message.MouseEnter += Lb_Message_MouseEnter;
                Lb_Message.MouseLeave += Lb_Message_MouseLeave;
            }
        }

        private void MessageOverlay_Load(object sender, EventArgs e) {
            T_AutoClose.Start();
        }

        private void T_AutoClose_Tick(object sender, EventArgs e) => CloseMessage();

        private void Lb_Message_MouseEnter(object sender, EventArgs e) {
            if(!MouseInBoundsSinceStart && HideOnHover) {
                CloseMessage();
            }
        }

        private void Lb_Message_MouseLeave(object sender, EventArgs e) {
            MouseInBoundsSinceStart = false;
        }

        public void CloseMessage() {
            T_AutoClose.Stop();
            CurrentOverlay = null;
            this.Close();
        }
    }
}