using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerInterface
{
    public static class PersistentQueue
    {

        public static void SaveQueue(Playlist playlist, AppSettings settings)
        {
            settings.QueuedSongs = playlist.Queue.Select(s => s.Path).ToArray();
            settings.QueueIndex = playlist.QueueIndex;
        }

        public static async Task RestoreQueue(Playlist playlist, AppSettings settings)
        {
            if(settings.QueuedSongs.Length > 0)
            {
                var queuedSongs = SongPathsHelper.CreateSongs(settings.QueuedSongs);
                await playlist.AddSongsAsync(queuedSongs);

                foreach(var qp in settings.QueuedSongs)
                {
                    if(playlist.FirstOrDefault(s => s.Path.Equals(qp, StringComparison.OrdinalIgnoreCase)) is Song q)
                    {
                        playlist.Enqueue(q);
                    }
                }

                if(playlist.Queue.Count > 0)
                {
                    if(settings.QueueIndex.HasValue)
                    {
                        do
                        {
                            playlist.Next(true);
                        }
                        while((playlist.QueueIndex ?? 0) < settings.QueueIndex.Value);
                    }
                    else
                    {
                        playlist.Next(true);
                    }
                }
            }

            settings.QueuedSongs = new string[0];
            settings.QueueIndex = null;
        }
    }
}
