using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace PlayerInterface.ViewModels {

    public class FullPlayerViewModel : NotifyPropertyChanged {
        
        public SongPlayer SongPlayer { get; }

        public string TrackLengthStr => FormatHelper.FormatTimeSpan(SongPlayer.TrackLength);

        public string StatusText => $"{SongPlayer?.CurrentSong?.Title} - {SongPlayer?.CurrentSong?.Artist}";

        public Visibility ShowDropHint => (playlist?.Length ?? 0) > 0 ? Visibility.Collapsed : Visibility.Visible;
        
        private Playlist playlist { get; }
        
        public AppSettings Settings { get; }

        public PlayingVm Playing { get; }

        public NextPrevVm NextPrev { get; }

        public PlaylistVm Playlist { get; }

        public ICommand PlaySongCommand { get; }
        
        public string PlaylistStats => $"{playlist?.Length} - {FormatHelper.FormatTimeSpan(new TimeSpan(playlist?.Sum(s => s.File.TrackLength.Ticks) ?? 0))}";

        public AppSettingsViewModel SettingsViewModel { get; }

        private bool _UIEnabled;

        public bool UIEnabled {
            get { return _UIEnabled; }
            set {
                if(_UIEnabled != value) {
                    _UIEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FullPlayerViewModel(AppSettings settings, SongPlayer player, Playlist playlist, PlayingVm playingVm, NextPrevVm nextPrevVm) {
            Settings = settings;
            this.playlist = playlist;
            SongPlayer = player;
            Playing = playingVm;
            NextPrev = nextPrevVm;

            Playlist = new PlaylistVm(Settings, playlist, player, e => UIEnabled = e, s => PlaySongCommand.Execute(s));
            SettingsViewModel = new AppSettingsViewModel(Settings);

            PlaySongCommand = new AsyncCommand<Song>(
                execute: inp => StartPlayingAsync(inp),
                canExecute: s => SongPlayer != null && s != null
            );

            playlist.ListContentChanged += (s, a) => {
                RaisePropertyChanged(nameof(ShowDropHint), nameof(PlaylistStats));
            };
            
            SongPlayer.SongChanged += SongPlayer_SongChanged;

            UIEnabled = true;
        }

        private void SongPlayer_SongChanged(object sender, PlayerCore.PlayerEventArgs.SongChangedEventArgs e) {
            RaisePropertyChanged(nameof(TrackLengthStr), nameof(StatusText));
            Playlist.SearchText = string.Empty;
        }

        private void StartPlaying(Song s) {
            playlist.SelectFirstMatch(s);
            if (SongPlayer.PlayerState != PlayerState.Playing) {
                SongPlayer.PlayerState = PlayerState.Playing;
            }
        }

        private async Task StartPlayingAsync(Song s) {
            UIEnabled = false;
            await Task.Run(() => StartPlaying(s));
            UIEnabled = true;
        }
    }
}