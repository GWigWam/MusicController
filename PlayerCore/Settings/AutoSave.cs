using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public class AutoSave {
        public SettingsFile File { get; }

        public Timer Timer { get; protected set; }

        public AutoSave(SettingsFile file, int intervalSec) {
            File = file;
            Timer = new Timer(Tick, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSec));
        }

        private void Tick(object state) {
            File.WriteToDisc();
        }
    }
}