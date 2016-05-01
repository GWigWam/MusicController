using PlayerCore.Settings;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

    public class TransitionManager {
        private uint SongDelayMs => Settings.SongTransitionDelayMs;

        private const bool Loop = true;

        private AppSettings Settings {
            get;
        }

        public SongPlayer Player {
            get; private set;
        }

        public Playlist TrackList {
            get; private set;
        }

        public TransitionManager(SongPlayer player, Playlist trackList, AppSettings settings) {
            Player = player;
            TrackList = trackList;
            Settings = settings;

            if(Player != null && Player.CurrentSong == null) {
                Player.CurrentSong = TrackList?.CurrentSong;
            }

            Init();
        }

        private void Init() {
            Player.PlayingStopped += Player_PlayingStopped;
            TrackList.CurrentSongChanged += TrackList_CurrentSongChanged;
        }

        private void Player_PlayingStopped(object sender, PlayingStoppedEventArgs args) {
            if(args.PlayedToEnd) {
                var curIndex = TrackList.CurrentSongIndex;
                TrackList.CurrentSongIndex = TrackList.HasNext ? (curIndex + 1) : (Loop ? (0) : (curIndex));
                Player.PlayerState = PlayerState.Paused;
                Task.Delay((int)SongDelayMs).ContinueWith((t) => {
                    Player.PlayerState = PlayerState.Playing;
                });
            }
        }

        private void TrackList_CurrentSongChanged(object sender, EventArgs e) {
            try {
                var newCur = TrackList.CurrentSong;
                if(newCur != null) {
                    Player.CurrentSong = newCur;
                } else {
                    Player.Stop();
                }
            } catch(SongLoadFailedException slfe) {
                if(slfe.Song != null && TrackList.Contains(slfe.Song)) {
                    TrackList.Remove(slfe.Song);
                }
            } catch(Exception) {
                // ---
            }
        }
    }
}