using PlayerCore;
using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels
{
    public class VolumeVm : NotifyPropertyChanged
    {
        public double MasterVolume {
            get => Math.Round(Volume.Decibel.ToLinear(Settings.MasterVolumeDb), 2);
            set {
                var db = Volume.Linear.ToDecibel(value);
                if(Settings.MasterVolumeDb != Math.Round(db, 3))
                {
                    Settings.MasterVolumeDb = db;
                }
            }
        }

        public string VolumeStr => $"{MasterVolume} ({Settings.MasterVolumeDb:N2}dB)";

        private AppSettings Settings { get; }

        public VolumeVm(AppSettings settings)
        {
            Settings = settings;

            Settings.Changed += (s, a) => {
                if(a.ChangedPropertyName == nameof(AppSettings.MasterVolumeDb))
                {
                    RaisePropertyChanged(nameof(MasterVolume), nameof(VolumeStr));
                }
            };
        }
    }
}
