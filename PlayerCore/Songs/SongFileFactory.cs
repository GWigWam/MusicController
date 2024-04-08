using PlayerCore.Persist;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.Songs
{
    public class SongFileFactory : IDisposable
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private readonly ConcurrentQueue<Song> UpdateTagsQueue = new();

        private AppSettings AppSettings { get; }
        private FileTagsCache FileTagsCache { get; }

        public SongFileFactory(AppSettings appSettings, FileTagsCache tagsCache)
        {
            AppSettings = appSettings;
            FileTagsCache = tagsCache;
            _ = RunBackgroundTagLoader(CancellationTokenSource.Token);
        }

        public Task<Song?> CreateAsync(string filePath) => Task.Run(() => Create(filePath));

        public Song? Create(string filePath)
        {
            var file = new FileInfo(filePath);
            if(file.Exists && SongPlayer.SupportedExtensions.Any(s => s.Equals(file.Extension, StringComparison.OrdinalIgnoreCase)))
            {
                var stats = AppSettings.GetSongStats(filePath);
                var tags = FileTagsCache.TryGet(filePath, out var tv) ? tv : null;
                var result = new Song(file.FullName, tags?.Title ?? file.Name.Replace(file.Extension, ""), tags, stats);
                UpdateTagsQueue.Enqueue(result);
                return result;
            }
            return null;
        }

        private async Task RunBackgroundTagLoader(CancellationToken ct)
        {
            while (true)
            {
                UpdateQueuedSongTags(ct);
                await Task.Delay(10, ct).ConfigureAwait(false);
            }
        }

        private void UpdateQueuedSongTags(CancellationToken ct)
        {
            while (UpdateTagsQueue.TryDequeue(out var song))
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var fileInfo = TagLib.File.Create(song.Path);
                    song.Tags = new SongTags
                    {
                        Title = fileInfo.Tag.Title,
                        TrackLength = fileInfo.Properties.Duration,
                        BitRate = fileInfo.Properties.AudioBitrate,

                        Artist = fileInfo.Tag.FirstPerformer,
                        Album = fileInfo.Tag.Album,
                        Genre = fileInfo.Tag.FirstGenre,

                        Track = fileInfo.Tag.Track is var t and not 0 ? (int)t : null,
                        TrackCount = fileInfo.Tag.TrackCount is var tc and not 0 ? (int)tc : null,
                        Disc = fileInfo.Tag.Disc is var d and not 0 ? (int)d : null,
                        DiscCount = fileInfo.Tag.DiscCount is var dc and not 0 ? (int)dc : null,

                        Year = fileInfo.Tag.Year is var y and not 0 ? (int)y : null,

                        AlbumGain = fileInfo.Tag.ReplayGainAlbumGain is var ag and not double.NaN ? ag : null,
                        TrackGain = fileInfo.Tag.ReplayGainTrackGain is var tg and not double.NaN ? tg : null,
                    };
                    if (AppSettings.TryFixSongStatsByTagsHash(song.Path, song.Tags) is bool statsChanged && statsChanged)
                    {
                        song.Stats = AppSettings.GetSongStats(song.Path);
                    }
                    FileTagsCache.AddOrUpdate(song.Path, song.Tags);
                }
                catch (Exception) { }
            }
        }

        public void Dispose()
        {
            CancellationTokenSource?.Cancel();
        }
    }
}
