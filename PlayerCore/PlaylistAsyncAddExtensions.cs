using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore
{
    public static class PlaylistAsyncAddExtensions
    {
        public static async Task<Song[]> AddSongsAsync(this Playlist playlist, IAsyncEnumerable<Song> addSongs, Song? position = null, IEnumerable<Func<Song, object>>? orderBys = null)
        {
            const int minBatchSize = 8;
            const int maxBatchSize = 256;

            var added = new List<Song>();
            var batch = new List<Song>();
            var batchSize = 1;

            // Add songs in ever growing batches to spare the UI thread
            void addBatch()
            {
                added.AddRange(playlist.AddSongs(batch, position));
                position = added.LastOrDefault() ?? position;
                batch.Clear();
                batchSize *= 2;
            }

            await foreach(var add in addSongs)
            {
                batch.Add(add);
                if((batch.Count >= batchSize && batch.Count >= minBatchSize) || batch.Count >= maxBatchSize)
                {
                    addBatch();
                }
            }
            addBatch();

            if(orderBys != null)
            {
                playlist.Order(orderBys, added);
            }

            return added.ToArray();
        }
    }
}
