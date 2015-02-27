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
        private const int ListeningTimeIncrement = 10000;

        private static ListeningTimer instance;
        public static ListeningTimer Instance {
            get {
                if (instance == null) {
                    instance = new ListeningTimer();
                }
                return instance;
            }
        }

        private bool Hidden = false;
        private long ListeningUntil = Environment.TickCount;

        public long TimeLeftMs {
            get {
                var dif = ListeningUntil - Environment.TickCount;
                return dif > 0 ? dif : 0;
            }
        }

        public int TimeLeftSec {
            get {
                return (int)Math.Floor(TimeLeftMs / 1000D);
            }
        }

        public bool IsListening {
            get {
                return TimeLeftSec > 0;
            }
        }

        public void IncrementTime(int TimeMS = ListeningTimeIncrement) {
            if (IsListening) {
                ListeningUntil += TimeMS;
            } else {
                ListeningUntil = Environment.TickCount + TimeMS;
            }
            Hidden = false;
            System.Media.SystemSounds.Beep.Play();
        }

        public void StopListening() {
            ListeningUntil = Environment.TickCount;
        }

        private ListeningTimer(int TimeUntilEnd = 0) {
            InitializeComponent();
            
            Screen screen = Screen.AllScreens[0];
            this.Left = screen.WorkingArea.Right - this.Width - 100;
            this.Top = screen.WorkingArea.Bottom - this.Height - 100;
            ListeningUntil = Environment.TickCount + TimeUntilEnd;
            UpdateText();
        }

        private void NotifyForm_MouseEnter(object sender, EventArgs e) {
            this.Hide();
            Hidden = true;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e) {
            if (!Hidden) {
                UpdateText();
                if (IsListening) {
                    this.Show();
                } else {
                    Hidden = true;
                    this.Hide();
                    System.Media.SystemSounds.Beep.Play();
                }
            }
        }

        private void UpdateText() {
            NotifyText.Text = TimeLeftSec.ToString();

            if (TimeLeftSec % 2 == 0) {
                NotifyText.ForeColor = Color.FromArgb(192, 192, 192);
            } else {
                NotifyText.ForeColor = Color.FromArgb(192, 128, 128);
            }
        }

        private void NotifyText_TextChanged(object sender, EventArgs e) {
            NotifyText.Left = (this.Width - NotifyText.Width) / 2;
            NotifyText.Top = (this.Height - NotifyText.Height) / 2;
        }
    }
}
