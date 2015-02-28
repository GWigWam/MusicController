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
    public partial class MainForm : Form {
        SpeechInput speechInput;
        bool Listening = true;

        public MainForm() {
            InitializeComponent();

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.ActiveControl = KeyInput;
            MoveToDefaultLocation();
        }

        private void Form1_Load(object sender, EventArgs e) {
            speechInput = new SpeechInput(this);
            speechInput.Start();

            var source = new AutoCompleteStringCollection();
            source.AddRange(SpeechInput.KEYWORDS);
            source.AddRange(MusicList.GetAllSongKeywords());
            KeyInput.AutoCompleteCustomSource = source;
            KeyInput.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            KeyInput.AutoCompleteSource = AutoCompleteSource.CustomSource;
            foreach (var word in SpeechInput.KEYWORDS) {
                Write(word + ", ");
            }
            WriteLine("Possible: ");
        }

        private void frmMain_Resize(object sender, EventArgs e) {
            if(FormWindowState.Minimized == this.WindowState) {
                HideWindow();
            } else {
                ShowWindow();
            }
        }

        private void KeyInput_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                speechInput.ExecuteCommand(KeyInput.Text.ToLower());
                KeyInput.Text = "";
            } else if (e.KeyCode == Keys.Escape) {
                HideWindow();
            }
        }

        private void Form1_Deactivate(object sender, EventArgs e) {
            HideWindow();
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ShowWindow();
            }
        }

        private void ShowWindow() {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            MoveToDefaultLocation();
            this.Activate();
        }

        private void HideWindow() {
            this.Hide();
            this.WindowState = FormWindowState.Minimized;
        }

        private void MoveToDefaultLocation() {
            Screen screen = Screen.AllScreens[0];
            this.Left = screen.WorkingArea.Right - this.Width;
            this.Top = screen.WorkingArea.Bottom - this.Height;
        }

        public void Write(string message) {
            textBox1.Text = message + textBox1.Text;
        }

        public void WriteLine(string message) {
            Write("\r\n" + message);
        }

        private void MenuItemShow_Click(object sender, EventArgs e) {
            ShowWindow();
        }

        private void MenuItemExit_Click(object sender, EventArgs e) {
            Environment.Exit(0);
        }
    }
}
