using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {

    public class TransitionManager {

        //TODO: Get from settings
        private const uint SongDelayMs = 2000;

        private const bool Loop = true;

        public SongPlayer Player {
            get; private set;
        }

        public Playlist TrackList {
            get; private set;
        }

        public TransitionManager(SongPlayer player, Playlist trackList) {
            Player = player;
            TrackList = trackList;
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