using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.Settings
{
    public delegate void StartupSongChangedHandler(object sender, StartupSongsChangedArgs args);

    public class StartupSongsChangedArgs
    {
        public bool IsStartupSong { get; }
        public string SongPath { get; }

        public StartupSongsChangedArgs(bool isStartupSong, string songPath) {
            IsStartupSong = isStartupSong;
            SongPath = songPath;
        }
    }
}
