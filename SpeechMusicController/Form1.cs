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
    public partial class Form1 : Form {

        SpeechInput speechInput;
        bool Listening = true;

        public Form1() {
            InitializeComponent();
            speechInput = new SpeechInput(this);
            speechInput.Start();
            NotifyIcon.Visible = false;
            WriteLine("");
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.ActiveControl = KeyInput;
        }

        private void Form1_Load(object sender, EventArgs e) {
            var source = new AutoCompleteStringCollection();
            source.AddRange(SpeechInput.KEYWORDS);
            source.AddRange(MusicList.GetAllSongAndBandNames().ToArray());
            KeyInput.AutoCompleteCustomSource = source;
            KeyInput.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            KeyInput.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void btnSwitch_Click(object sender, EventArgs e) {
            if(Listening) {
                Listening = false;
                btnSwitch.Text = "Start";
                speechInput.Enabled = false;
            } else {
                Listening = true;
                btnSwitch.Text = "Stop";
                speechInput.Enabled = true;
            }
        }

        public void WriteLine(string message) {
            textBox1.Text = "\r\n" + message + textBox1.Text;
        }

        private void frmMain_Resize(object sender, EventArgs e) {
            if(FormWindowState.Minimized == this.WindowState) {
                NotifyIcon.Visible = true;
                this.Hide();
                this.ShowInTaskbar = false;

                //NotifyIcon.BalloonTipTitle = "Minimize to Tray App";
                //NotifyIcon.BalloonTipText = "SpeechMusicController is down here";
                //NotifyIcon.ShowBalloonTip(500);
            } else if(FormWindowState.Normal == this.WindowState) {
                NotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void MenuItemExit_Click(object sender, EventArgs e) {
            Environment.Exit(0);
        }

        private void KeyInput_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                speechInput.ExecuteCommand(KeyInput.Text.ToLower());
                KeyInput.Text = "";
            } else if (e.KeyCode == Keys.Escape) {
                this.WindowState = FormWindowState.Minimized;
                frmMain_Resize(null, null);
            }
        }
    }
}
