using PlayerCore.Persist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Api.Enums;
using PlayerCore.Songs;

#nullable enable
namespace PlayerCore.Scrobbling
{
    public class Scrobbler
    {
        private const string ApiKey = "53af976151f67320af09b36ff7555e53";
        private const string SharedSecret = "2808f4e23c4a077c8af93bc01a4328bc";

        public event Action<Exception>? ExceptionOccured;

        private AppSettings Settings { get; }

        private LastfmClient? Client { get; set; }

        public bool CanScrobble => Settings.LastfmAuthed;

        private Dictionary<Song, DateTime> StatLookupHistory { get; } = new();

        public Scrobbler(AppSettings settings, SongPlayer player)
        {
            Settings = settings;
            SetupScrobbling(player);
        }

        public async Task Login(string username, string password)
        {
            var client = new LastfmClient(ApiKey, SharedSecret);
            var resp = await client.Auth.GetSessionTokenAsync(username, password);
            if (resp.Success)
            {
                SecureSessionStore.Save(username, client.Auth.UserSession.Token);
                Settings.LastfmAuthed = true;
            }
            else
            {
                throw new Exception($"Auth failed: {resp.Status}");
            }
        }

        public void Logout()
        {
            Client = null;
            Settings.LastfmAuthed = false;
            Settings.LastfmUser = "";
            SecureSessionStore.Clear();
        }

        public async Task Scrobble(Song song)
        {
            if (!string.IsNullOrEmpty(song.Tags?.Artist) && !string.IsNullOrEmpty(song.Tags.Album) && !string.IsNullOrEmpty(song.Tags.Title))
            {
                Client ??= CreateAuthedClient();

                var resp = await Client.Scrobbler.ScrobbleAsync(SongToScrobble(song.Tags));
                if (!resp.Success)
                {
                    if (resp.Status == LastResponseStatus.BadAuth || resp.Status == LastResponseStatus.SessionExpired || resp.Status == LastResponseStatus.BadApiKey || resp.Status == LastResponseStatus.KeySuspended)
                    {
                        Logout();
                    }
                    throw resp.Exception ?? new Exception($"Scrobbling failure: {resp.Status}");
                }
            }
        }

        public async Task NowPlaying(Song song)
        {
            if (!string.IsNullOrEmpty(song.Tags?.Artist) && !string.IsNullOrEmpty(song.Tags.Album) && !string.IsNullOrEmpty(song.Tags.Title))
            {
                Client ??= CreateAuthedClient();
                await Client.Track.UpdateNowPlayingAsync(SongToScrobble(song.Tags));
            }
        }

        public async Task UpdateStats(Song song)
        {
            if (!StatLookupHistory.TryGetValue(song, out var lastLookup) || lastLookup + TimeSpan.FromMinutes(30) < DateTime.Now)
            {
                StatLookupHistory[song] = DateTime.Now;
                Client ??= CreateAuthedClient();

                if (Settings.GetSongStats(song) is SongStats stats &&
                    (song.Tags?.Title is string title && song.Tags.Artist is string artist) &&
                    (await Client.Track.GetInfoAsync(title, artist, Client.User.Auth.UserSession.Username)) is { Success: true } info)
                {
                    stats.PlayCount = info.Content.UserPlayCount is int pc && pc > stats.PlayCount ? pc : stats.PlayCount;
                }
            }
        }

        private static Scrobble SongToScrobble(SongTags song)
            => new Scrobble(song.Artist, song.Album, song.Title, DateTimeOffset.Now) { Duration = song.TrackLength };

        private LastfmClient CreateAuthedClient()
        {
            if (Settings.LastfmAuthed)
            {
                try
                {
                    var client = new LastfmClient(ApiKey, SharedSecret);
                    var (usr, sesTkn) = SecureSessionStore.Load();
                    if (string.Equals(usr, Settings.LastfmUser, StringComparison.OrdinalIgnoreCase))
                    {
                        if (client.Auth.LoadSession(new LastUserSession { Username = usr, Token = sesTkn }) && client.Auth.Authenticated)
                        {
                            return client;
                        }
                        else
                        {
                            throw new Exception("Failed to load stored session");
                        }
                    }
                    else
                    {
                        throw new Exception("No credentials stored for this user");
                    }
                }
                catch (Exception)
                {
                    Logout();
                    throw;
                }
            }
            else
            {
                throw new Exception("Application is not authenticated");
            }
        }

        private void SetupScrobbling(SongPlayer player)
        {
            var (delayed, scrobbled) = ((Song?)null, (Song?)null);

            player.PlaybackStateChanged += (_, _) => _ = tryProcessBackground();
            player.SongChanged += (s, a) => {
                if (a.Next != a.Previous)
                {
                    (delayed, scrobbled) = (null, null);
                    _ = tryProcessBackground();
                }
            };

            async Task tryProcessBackground() { try { if (CanScrobble) { await processBackground(); } } catch (Exception) { /* Fail silently */ } }
            async Task processBackground()
            {
                var handle = player.CurrentSong;
                await Task.Delay(1000);

                if (player.IsPlaying && player.CurrentSong is Song playing && playing == handle && playing.Tags?.TrackLength is TimeSpan duration)
                {
                    if (delayed != playing)
                    {
                        delayed = playing;
                        await UpdateStats(playing);
                        await NowPlaying(playing);
                    }

                    const int scrobbleDelayMin = 4;
                    const double scrobblePercentage = 0.9;
                    var scrobbleDelay = Math.Min((duration - player.Elapsed).TotalMilliseconds * scrobblePercentage, scrobbleDelayMin * 60 * 1000); // Scrobble after 4min or after 90% of song, whichever is first
                    await Task.Delay((int)scrobbleDelay);

                    if (player.IsPlaying && player.CurrentSong is Song scrobble && scrobble == handle && scrobbled != scrobble)
                    {
                        scrobbled = scrobble;
                        await Scrobble(scrobble);
                    }
                }
            }
        }
    }
}
