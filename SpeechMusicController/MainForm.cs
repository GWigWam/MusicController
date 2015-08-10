using SpeechMusicController.AppSettings;
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

        public MainForm() {
            InitializeComponent();

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.ActiveControl = KeyInput;
            MoveToDefaultLocation();
        }

        private void Form1_Load(object sender, EventArgs e) {
            foreach (var word in SpeechInput.Keywords) {
                Write(word + ", ");
            }
            WriteLine("Possible: ");

            UpdateSuggestions();

            SpeechInput.MessageSend += (s) => {
                Action<string> update = WriteLine;
                Invoke(update, s);
            };
            Settings.Instance.OnRulesChanged += () => {
                Action update = UpdateSuggestions;
                Invoke(update);
            };
            MusicList.SongListUpdated += () => {
                Action update = UpdateSuggestions;
                Invoke(update);
            };
        }

        private void UpdateSuggestions() {
            var source = new AutoCompleteStringCollection();
            source.AddRange(SpeechInput.Keywords.ToArray());
            source.AddRange(MusicList.GetAllSongKeywords());
            KeyInput.AutoCompleteCustomSource.Clear();
            KeyInput.AutoCompleteCustomSource = source;
            KeyInput.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            KeyInput.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void frmMain_Resize(object sender, EventArgs e) {
            if (FormWindowState.Minimized == this.WindowState) {
                HideWindow();
            } else {
                ShowWindow();
            }
        }

        private void KeyInput_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                SpeechInput.ExecuteCommand(KeyInput.Text.ToLower());
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
            Tb_Output.Text = message + Tb_Output.Text;
        }

        public void WriteLine(string message) {
            Write("\r\n" + message);
        }

        private void MenuItemShow_Click(object sender, EventArgs e) {
            ShowWindow();
        }

        private void MenuItemExit_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void Bt_Rules_Click(object sender, EventArgs e) {
            var rulesEdit = new RulesEdit();
            rulesEdit.Show();
        }

        #region Refresh

        private void Bt_Refresh_Click(object sender, EventArgs e) {
            UpdateMusicList();
        }

        private void MenuItemRefresh_Click(object sender, EventArgs e) {
            UpdateMusicList();
        }

        private async void UpdateMusicList() {
            Bt_Refresh.Enabled = false;
            KeyInput.Enabled = false;
            KeyInput.Text = "Refreshing...";

            await Task.Run(() => {
                MusicList.ReadListFromDisc();
            });

            KeyInput.Text = "";
            KeyInput.Enabled = true;
            Bt_Refresh.Enabled = true;
        }

        #endregion Refresh

        private void Bt_Settings_Click(object sender, EventArgs e) {
            var sysVar = new SystemVarsEdit();
            sysVar.Show();
        }
    }
}