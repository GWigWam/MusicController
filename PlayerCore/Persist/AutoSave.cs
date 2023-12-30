using System;
using System.Threading;

namespace PlayerCore.Persist
{
    public class AutoSave
    {
        public PersistToFile File { get; }

        public Timer Timer { get; protected set; }

        public AutoSave(PersistToFile file, int intervalSec) {
            File = file;
            Timer = new Timer(Tick, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSec));
        }

        private async void Tick(object state) {
            await File.WriteToDiscAsync();
        }
    }
}
