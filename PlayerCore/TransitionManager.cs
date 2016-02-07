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
            Player.SongEnded += Player_SongEnded;
            TrackList.CurrentSongChanged += TrackList_CurrentSongChanged;
        }

        private async void Player_SongEnded(object sender, Song prevSong) {
            await Task.Delay((int)SongDelayMs);

            if(TrackList.CurrentSongIndex < TrackList.Length - 1) {
                TrackList.CurrentSongIndex++;
            } else if(Loop) {
                TrackList.CurrentSongIndex = 0;
            }
        }

        private void TrackList_CurrentSongChanged(object sender, EventArgs e) {
            var newCur = TrackList.CurrentSong;
            if(newCur != null) {
                Player.CurrentSong = newCur;
            } else {
                Player.Stop();
            }
        }
    }
}