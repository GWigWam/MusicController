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
            settings.QueueIndex = playlist.QueueIndex;
        }

        public static void RestoreQueue(Playlist playlist, AppSettings settings) {
            foreach (var songPath in settings.QueuedSongs ?? new string[0]) {
                var foundSong = playlist.FirstOrDefault(s => s.FilePath == songPath);
                if (foundSong != null) {
                    playlist.Enqueue(foundSong);
                } else {
                    var loaded = PlayerCore.Songs.SongFileReader.CreateSongs(settings, new[] { songPath });
                    if (loaded.Length == 1) {
                        playlist.Enqueue(loaded[0]);
                    }
                }
            }

            if (playlist.Queue.Count > 0 && settings.QueueIndex.HasValue) {
                do {
                    playlist.Next(true);
                } while ((playlist.QueueIndex ?? 0) < settings.QueueIndex.Value);
            }

            settings.QueuedSongs = new string[0];
            settings.QueueIndex = null;
        }
    }
}
