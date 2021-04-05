using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
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
                    if(SongFile.TryCreate(songPath, out var sf)) {
                        var song = new Song(sf);
                        playlist.Enqueue(song);
                    }
                }
            }

            if (playlist.Queue.Count > 0) {
                if(settings.QueueIndex.HasValue) {
                    do {
                        playlist.Next(true);
                    } while((playlist.QueueIndex ?? 0) < settings.QueueIndex.Value);
                } else {
                    playlist.Next(true);
                }
            }

            settings.QueuedSongs = new string[0];
            settings.QueueIndex = null;
        }
    }
}
