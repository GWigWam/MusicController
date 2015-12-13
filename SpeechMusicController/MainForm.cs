using SpeechMusicController.AppSettings;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeechMusicController {

    public partial class MainForm : Form {
        private Settings AppSettings;
        private SongRules CurrentSongRules;
        private MusicList MusicCollection;
        private SpeechInput SpeechControll;

        public MainForm(Settings settings) {
            AppSettings = settings;
            InitializeComponent();

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            ActiveControl = KeyInput;
            MoveToDefaultLocation();
        }

        private void Form1_Load(object sender, EventArgs e) => Init();

        private void Init() {
            string rulesPath;
            if(AppSettings.TryGetSetting("SongRulesPath", out rulesPath) && !string.IsNullOrEmpty(rulesPath)) {
                CurrentSongRules = SettingsFile.ReadSettingFile<SongRules>(rulesPath);

                if(CurrentSongRules == null) {
                    File.Delete(rulesPath);
                    CurrentSongRules = new SongRules(rulesPath);
                }
            } else {
                MessageBox.Show("Error: RulesPath setting is empty");
                return;
            }

            MusicCollection = new MusicList(AppSettings, CurrentSongRules);
            WriteLine($"Loaded {MusicCollection.ActiveSongs.Count()} songs");

            string playerPath;
            if(AppSettings.TryGetSetting("PlayerPath", out playerPath) && !string.IsNullOrEmpty(playerPath)) {
                SpeechControll = new SpeechInput(AppSettings, MusicCollection, playerPath);
            } else {
                MessageBox.Show("Error: PlayerPath setting is empty");
                return;
            }

            foreach(var word in SpeechControll.Keywords) {
                Write(word + ", ");
            }
            WriteLine("Possible: ");

            UpdateSuggestions();

            SpeechControll.MessageSend += (s) => {
                Action<string> update = WriteLine;
                Invoke(update, s);
            };
            CurrentSongRules.OnChange += (s, a) => {
                Action update = UpdateSuggestions;
                Invoke(update);
            };
            MusicCollection.SongListUpdated += (s, a) => {
                Action update = UpdateSuggestions;
                Invoke(update);
            };
            new MessageOverlay("SpeechMusicController is listening!", 1500).Show();
        }

        private void UpdateSuggestions() {
            var source = new AutoCompleteStringCollection();
            source.AddRange(SpeechControll.Keywords.ToArray());
            source.AddRange(MusicCollection.GetAllSongKeywords());
            KeyInput.AutoCompleteCustomSource.Clear();
            KeyInput.AutoCompleteCustomSource = source;
            KeyInput.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            KeyInput.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void frmMain_Resize(object sender, EventArgs e) {
            if(FormWindowState.Minimized == this.WindowState) {
                HideWindow();
            } else {
                ShowWindow();
            }
        }

        private void KeyInput_KeyUp(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Enter) {
                SpeechControll.ExecuteCommand(KeyInput.Text.ToLower(), true);
                KeyInput.Text = "";
            } else if(e.KeyCode == Keys.Escape) {
                HideWindow();
            }
        }

        private void Form1_Deactivate(object sender, EventArgs e) => HideWindow();

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Left) {
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

        public void Write(string message) => Tb_Output.Text = message + Tb_Output.Text;

        public void WriteLine(string message) => Write("\r\n" + message);

        private void MenuItemShow_Click(object sender, EventArgs e) => ShowWindow();

        private void MenuItemExit_Click(object sender, EventArgs e) => Application.Exit();

        private void Bt_Rules_Click(object sender, EventArgs e) {
            var rulesEdit = new RulesEdit(CurrentSongRules, MusicCollection);
            rulesEdit.Show();
        }

        #region Refresh

        private void Bt_Refresh_Click(object sender, EventArgs e) => UpdateMusicList();

        private void MenuItemRefresh_Click(object sender, EventArgs e) => UpdateMusicList();

        private async void UpdateMusicList() {
            Bt_Refresh.Enabled = false;
            KeyInput.Enabled = false;
            KeyInput.Text = "Refreshing...";

            if(SpeechControll == null) {
                Init();
            }

            await Task.Run(() => {
                MusicCollection.ReadListFromDisc();
            });

            KeyInput.Text = "";
            KeyInput.Enabled = true;
            Bt_Refresh.Enabled = true;
        }

        #endregion Refresh

        private void Bt_Settings_Click(object sender, EventArgs e) {
            var sysVar = new SystemVarsEdit(AppSettings);
            sysVar.Show();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            CurrentSongRules?.WriteToDisc(false);
        }

        private void Bt_SpeechSwitch_Click(object sender, EventArgs e) {
            if(SpeechControll.Listening) {
                SpeechControll.Listening = false;
                Bt_SpeechSwitch.Text = "Speech (Off)";
            } else {
                SpeechControll.Listening = true;
                Bt_SpeechSwitch.Text = "Speech (On)";
            }
        }
    }
}