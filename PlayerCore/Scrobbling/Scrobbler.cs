using PlayerCore.Settings;
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
            if (!string.IsNullOrEmpty(song.Artist) && !string.IsNullOrEmpty(song.Album) && !string.IsNullOrEmpty(song.Title))
            {
                Client ??= CreateAuthedClient();

                var resp = await Client.Scrobbler.ScrobbleAsync(SongToScrobble(song));
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
            if (!string.IsNullOrEmpty(song.Artist) && !string.IsNullOrEmpty(song.Album) && !string.IsNullOrEmpty(song.Title))
            {
                Client ??= CreateAuthedClient();
                await Client.Track.UpdateNowPlayingAsync(SongToScrobble(song));
            }
        }

        private static Scrobble SongToScrobble(Song song)
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
            async Task scrobbleBackground(bool playedToEnd)
            {
                if (playedToEnd && player.CurrentSong != null && CanScrobble)
                {
                    try
                    {
                        await Scrobble(player.CurrentSong);
                    }
                    catch (Exception e)
                    {
                        ExceptionOccured?.Invoke(e);
                    }
                }
            }
            player.PlayingStopped += (_, a) => _ = scrobbleBackground(a.PlayedToEnd);
            
            async Task nowPlayingBackground()
            {
                if (player.CurrentSong != null && player.IsPlaying && CanScrobble)
                {
                    try
                    {
                        await NowPlaying(player.CurrentSong);
                    }
                    catch (Exception) { } // Fail silently
                }
            }
            player.PlaybackStateChanged += (s, a) => _ = nowPlayingBackground();
            player.SongChanged += (s, a) => _ = nowPlayingBackground();
        }
    }
}
