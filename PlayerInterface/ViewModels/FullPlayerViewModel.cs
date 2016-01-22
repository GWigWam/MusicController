using NAudio.Wave;
using PlayerCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class FullPlayerViewModel : SmallPlayerViewModel {
        private Timer UpdateTimer;

        public Song CurrentSong {
            get { return SongPlayer.CurrentSong; }
            set { SongPlayer.CurrentSong = value; }
        }

        public string Elapsed => FormatTimeSpan(SongPlayer.Elapsed);

        public string TrackLength => FormatTimeSpan(SongPlayer.TrackLength);

        public FullPlayerViewModel(SongPlayer player) : base(player) {
            UpdateTimer = new Timer() {
                AutoReset = true,
                Enabled = true,
                Interval = 1000
            };
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e) {
            RaisePropertiesChanged("Elapsed");
        }
    }
}