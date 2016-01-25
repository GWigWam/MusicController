using NAudio.Wave;
using PlayerCore;
using PlayerCore.Songs;
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

        //Slider should not all the way to end of end of track, track should end 'naturaly'
        private int SliderTrackEndBufferMs = 500;

        public Song CurrentSong {
            get { return SongPlayer.CurrentSong; }
            set { SongPlayer.CurrentSong = value; }
        }

        public string ElapsedStr => FormatTimeSpan(SongPlayer.Elapsed);

        public string TrackLengthStr => FormatTimeSpan(SongPlayer.TrackLength);

        public double ElapsedFraction {
            get { return SongPlayer.Elapsed.TotalMilliseconds / (SongPlayer.TrackLength.TotalMilliseconds - SliderTrackEndBufferMs); }
            set {
                if(value >= 0 && value <= 1) {
                    var miliseconds = (SongPlayer.TrackLength.TotalMilliseconds - SliderTrackEndBufferMs) * value;
                    var newTime = TimeSpan.FromMilliseconds(miliseconds);
                    SongPlayer.Elapsed = newTime;
                }
            }
        }

        public FullPlayerViewModel(SongPlayer player) : base(player) {
            UpdateTimer = new Timer() {
                AutoReset = true,
                Enabled = true,
                Interval = 1000
            };
            SongPlayer.SongChanged += SongPlayer_SongChanged;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private void SongPlayer_SongChanged(object sender, EventArgs e) {
            RaisePropertiesChanged(nameof(ElapsedStr), nameof(ElapsedFraction), nameof(TrackLengthStr));
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e) {
            RaisePropertiesChanged(nameof(ElapsedStr), nameof(ElapsedFraction));
        }
    }
}