using PlayerCore;
using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        public VolumeVm Volume { get; }

        public ICommand PlaySongCommand { get; }
        
        public string PlaylistStats => $"{playlist?.Length} - {FormatHelper.FormatTimeSpan(new TimeSpan(playlist?.Sum(s => s.TrackLength.Ticks) ?? 0))}";

        public AppSettingsViewModel SettingsViewModel { get; }

        public FullPlayerViewModel(AppSettings settings, SongPlayer player, Playlist playlist, PlayingVm playingVm, NextPrevVm nextPrevVm) {
            Settings = settings;
            this.playlist = playlist;
            SongPlayer = player;
            Playing = playingVm;
            NextPrev = nextPrevVm;

            Playlist = new PlaylistVm(settings, playlist, player, s => PlaySongCommand.Execute(s));
            SettingsViewModel = new AppSettingsViewModel(settings);
            Volume = new VolumeVm(settings);

            PlaySongCommand = new AsyncCommand<Song>(
                execute: inp => StartPlayingAsync(inp),
                canExecute: s => SongPlayer != null && s != null
            );

            playlist.CollectionChanged += (s, a) => {
                if(a.Action == NotifyCollectionChangedAction.Add || a.Action == NotifyCollectionChangedAction.Remove || a.Action == NotifyCollectionChangedAction.Reset)
                {
                    RaisePropertyChanged(nameof(ShowDropHint), nameof(PlaylistStats));
                }
            };
            
            SongPlayer.SongChanged += SongPlayer_SongChanged;
        }

        private void SongPlayer_SongChanged(object sender, PlayerCore.PlayerEventArgs.SongChangedEventArgs e) {
            RaisePropertyChanged(nameof(TrackLengthStr), nameof(StatusText));
            Playlist.SearchText = string.Empty;
        }

        private void StartPlaying(Song s) {
            playlist.SelectFirstMatch(s);
            if (!SongPlayer.IsPlaying) {
                SongPlayer.Play();
            }
        }

        private Task StartPlayingAsync(Song s)
            => Task.Run(() => StartPlaying(s));
    }
}