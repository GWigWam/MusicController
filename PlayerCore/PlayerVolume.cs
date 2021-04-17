using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore
{
    public class PlayerVolume
    {
        public event EventHandler? VolumeChanged;

        public double _MasterVolumeDb = 0;
        public double MasterVolumeDb {
            get => _MasterVolumeDb;
            set {
                if(value != _MasterVolumeDb)
                {
                    _MasterVolumeDb = value;
                    RaiseChanged();
                }
            }
        }
        
        public double MasterVolume {
            get => Volume.Decibel.ToLinear(MasterVolumeDb);
            set => MasterVolumeDb = Volume.Linear.ToDecibel(value);
        }
        
        private double? _GainDb = null;
        public double? GainDb {
            get => _GainDb;
            set {
                if(value != _GainDb)
                {
                    _GainDb = value;
                    RaiseChanged();
                }
            }
        }

        public double? Gain {
            get => GainDb is double db ? Volume.Decibel.ToLinear(db) : null;
            set => GainDb = (value is double lin ? Volume.Linear.ToDecibel(lin) : null);
        }

        public double _GainPreampDb = 0;
        public double GainPreampDb {
            get => _GainPreampDb;
            set {
                if(value != _GainPreampDb)
                {
                    _GainPreampDb = value;
                    RaiseChanged();
                }
            }
        }

        public double GainPreamp {
            get => Volume.Decibel.ToLinear(GainPreampDb);
            set => GainPreampDb = Volume.Linear.ToDecibel(value);
        }

        public double OutputVolumeDb => CalcVolumeDb();
        public double OutputVolume => Volume.Decibel.ToLinear(OutputVolumeDb);

        private void RaiseChanged() => VolumeChanged?.Invoke(this, EventArgs.Empty);

        private double CalcVolumeDb() => MasterVolumeDb + (GainDb is double gain ? (gain + GainPreampDb) : 0);
    }
}
