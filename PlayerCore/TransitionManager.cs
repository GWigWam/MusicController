﻿using PlayerCore.PlayerEventArgs;
using PlayerCore.Persist;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore {

    public class TransitionManager : INotifyPropertyChanged
    {
        private int SongDelayMs => (int)Settings.SongTransitionDelayMs;

        private const bool Loop = false;

        private CancellationTokenSource? CancelSrc { get; set; }

        public event EventHandler<Exception>? SongLoadErrorOccurred;
        public event EventHandler? TransitionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private AppSettings Settings { get; }

        public SongPlayer Player { get; private set; }
        public Playlist TrackList { get; private set; }

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

        private bool _PauseAfterCurrent;
        public bool PauseAfterCurrent {
            get => _PauseAfterCurrent;
            set {
                if(_PauseAfterCurrent != value) {
                    _PauseAfterCurrent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PauseAfterCurrent)));
                }
            }
        }

        public TransitionManager(SongPlayer player, Playlist trackList, AppSettings settings)
        {
            Player = player;
            TrackList = trackList;
            Settings = settings;

            if(Player != null && Player.CurrentSong == null) {
                ChangeSongSafe(TrackList?.CurrentSong);
            }

            Init();
        }

        public void StartTransition() {
            var prev = TrackList.CurrentSong;
            var moved = TrackList.Next(Loop);
            if (moved) {
                Player.Pause();

                if(PauseAfterCurrent) {
                    PauseAfterCurrent = false;
                    return;
                }

                CancelSrc = new CancellationTokenSource();
                Task.Run(async () => {
                    try {
                        IsTransitioning = true;
                        var isNextOnAlbum = prev != null
                            && prev.Tags?.Album is string pAlbum && pAlbum == TrackList.CurrentSong!.Tags?.Album
                            && (prev.Tags?.Track == null || prev.Tags.Track + 1 == TrackList.CurrentSong.Tags?.Track)
                            && prev.Tags?.Disc == TrackList.CurrentSong.Tags?.Disc;
                        var delay = isNextOnAlbum ? 0 : SongDelayMs; // Do not delay between consecutive tracks on same album
                        await Task.Delay(delay, CancelSrc.Token);
                        Player.Play();
                    } finally {
                        IsTransitioning = false;
                    }
                }, CancelSrc.Token);
            } else {
                Player.Stop();
            }
        }

        public void CancelTransition() => CancelSrc?.Cancel();

        private void Init() {
            Player.PlayingStopped += Player_PlayingStopped;
            TrackList.CurrentSongChanged += TrackList_CurrentSongChanged;
        }

        private void Player_PlayingStopped(object? sender, PlayingStoppedEventArgs args) {
            if(args.PlayedToEnd) {
                StartTransition();
            }
        }

        private void TrackList_CurrentSongChanged(object? sender, EventArgs e) {
            try {
                var newCur = TrackList.CurrentSong;
                if(newCur != null) {
                    ChangeSongSafe(newCur);
                } else {
                    Player.Stop();
                }
            } catch(SongLoadFailedException slfe) {
                if(slfe.Song != null && TrackList.Contains(slfe.Song)) {
                    TrackList.Remove(slfe.Song);
                }
            }
        }

        private void ChangeSongSafe(Song? song)
        {
            try
            {
                Player.ChangeSong(song);
            }
            catch (Exception e)
            {
                SongLoadErrorOccurred?.Invoke(this, e);
            }
        }
    }
}
