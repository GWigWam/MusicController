using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public delegate void StartupSongChangedHandler(object sender, StartupSongsChangedArgs args);

    public class StartupSongsChangedArgs {
        public bool IsStartupSong { get; }
        public SongFile SongFile { get; }

        public StartupSongsChangedArgs(bool isStartupSong, SongFile songFile) {
            IsStartupSong = isStartupSong;
            SongFile = songFile;
        }
    }
}
