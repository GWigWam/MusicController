using PlayerCore.PlayerEventArgs;
using PlayerCore.Settings;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerCore {

    public class TransitionManager {
        private int SongDelayMs => (int)Settings.SongTransitionDelayMs;

        private const bool Loop = true;

        private CancellationTokenSource CancelSrc { get; set; }

        public event EventHandler TransitionChanged;

        private AppSettings Settings {
            get;
        }

        public SongPlayer Player {
            get; private set;
        }

        public Playlist TrackList {
            get; private set;
        }

        private bool isTransitioning;
        public bool IsTransitioning {
            get { return isTransitioning; }
            protected set {
                if(isTransitioning != value) {
                    isTransitioning = value;
                    TransitionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
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

        public void StartTransition(int delayMs) {
            var curIndex = TrackList.CurrentSongIndex;
            TrackList.CurrentSongIndex = TrackList.HasNext ? (curIndex + 1) : (Loop ? (0) : (curIndex));
            Player.PlayerState = PlayerState.Paused;

            CancelSrc = new CancellationTokenSource();
            Task.Run(async () => {
                try {
                    IsTransitioning = true;
                    await Task.Delay(SongDelayMs, CancelSrc.Token);
                    Player.PlayerState = PlayerState.Playing;
                } finally {
                    IsTransitioning = false;
                }
            }, CancelSrc.Token);
        }

        public void StartTransition() => StartTransition(SongDelayMs);

        public void CancelTransition() => CancelSrc?.Cancel();

        private void Init() {
            Player.PlayingStopped += Player_PlayingStopped;
            TrackList.CurrentSongChanged += TrackList_CurrentSongChanged;
        }

        private void Player_PlayingStopped(object sender, PlayingStoppedEventArgs args) {
            if(args.PlayedToEnd) {
                StartTransition();
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