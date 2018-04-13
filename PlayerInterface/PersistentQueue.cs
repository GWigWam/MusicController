using PlayerCore;
using PlayerCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface {
    public static class PersistentQueue {

        public static void SaveQueue(Playlist playlist, AppSettings settings) {
            settings.QueuedSongs = playlist.Queue.Select(s => s.FilePath).ToArray();
        }

        public static void RestoreQueue(Playlist playlist, AppSettings settings) {
            foreach (var songPath in settings.QueuedSongs ?? new string[0]) {
                var foundSong = playlist.FirstOrDefault(s => s.FilePath == songPath);
                if (foundSong != null) {
                    playlist.Enqueue(foundSong);
                } else {
                    var loaded = PlayerCore.Songs.SongFileReader.ReadFilePaths(settings, new[] { songPath });
                    if (loaded.Length == 1) {
                        playlist.Enqueue(loaded[0]);
                    }
                }
            }
            if (playlist.Queue.Count > 0 && (playlist.QueueIndex ?? 0) == 0) {
                playlist.Next(true);
            }

            settings.QueuedSongs = new string[0];
        }
    }
}
