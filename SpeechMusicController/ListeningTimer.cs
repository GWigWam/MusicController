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

    public partial class ListeningTimer : Form {
        private const int ListeningTimeIncrement = 6000;

        private bool Hidden;
        private long ListeningUntil;

        public long TimeLeftMs {
            get {
                var dif = ListeningUntil - Environment.TickCount;
                return dif > 0 ? dif : 0;
            }
        }

        public int TimeLeftSec => (int)Math.Floor(TimeLeftMs / 1000D);
        public bool IsListening => TimeLeftSec > 0;

        public void IncrementTime(int TimeMS = ListeningTimeIncrement) {
            System.Media.SystemSounds.Beep.Play();
            if(IsListening) {
                ListeningUntil += TimeMS;
            } else {
                ListeningUntil = Environment.TickCount + TimeMS;
                CountdownTimer.Start();
            }
            Hidden = false;
        }

        public void StopListening() => ListeningUntil = Environment.TickCount;

        public ListeningTimer() {
            InitializeComponent();
        }

        private void ListeningTimer_Load(object sender, EventArgs e) {
            Screen screen = Screen.AllScreens[0];
            this.Left = screen.WorkingArea.Right - this.Width - 100;
            this.Top = screen.WorkingArea.Bottom - this.Height - 100;
            UpdateText();
        }

        private void NotifyForm_MouseEnter(object sender, EventArgs e) {
            HideForm();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e) {
            if(!Hidden) {
                if(IsListening) {
                    UpdateText();
                    this.Show();
                } else {
                    HideForm();
                }
            }
        }

        private void UpdateText() {
            NotifyText.Text = TimeLeftSec.ToString();

            if(TimeLeftSec % 2 == 0) {
                NotifyText.ForeColor = Color.FromArgb(192, 192, 192);
            } else {
                NotifyText.ForeColor = Color.FromArgb(192, 128, 128);
            }
        }

        private void NotifyText_TextChanged(object sender, EventArgs e) {
            NotifyText.Left = (this.Width - NotifyText.Width) / 2;
            NotifyText.Top = (this.Height - NotifyText.Height) / 2;
        }

        private void HideForm() {
            Hidden = true;
            this.Hide();
            CountdownTimer.Stop();
            System.Media.SystemSounds.Beep.Play();
        }
    }
}