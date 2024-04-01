using PlayerCore;
using PlayerCore.Scrobbling;
using PlayerCore.Persist;
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

        public string StatusText => $"{SongPlayer?.CurrentSong?.Title} - {SongPlayer?.CurrentSong?.Tags?.Artist}";

        public Visibility ShowDropHint => (playlist?.Length ?? 0) > 0 ? Visibility.Collapsed : Visibility.Visible;
        
        private Playlist playlist { get; }
        
        public AppSettings Settings { get; }

        public PlayingVm Playing { get; }

        public NextPrevVm NextPrev { get; }

        public PlaylistVm Playlist { get; }

        public VolumeVm Volume { get; }

        public Scrobbler Scrobbler { get; }

        public ICommand PlaySongCommand { get; }
        public IBaseCommand LastfmLoginCmd { get; }
        public IBaseCommand LastfmLogoutCmd { get; }

        public string PlaylistStats => Playlist.SelectedPlaylistItems.ToArray() is { Length: > 1 } sel ?
            $"{sel.Length} - {FormatHelper.FormatTimeSpan(new TimeSpan(sel.Sum(svm => svm.Song.Tags?.TrackLength.Ticks ?? 1)))}" :
            $"{playlist.Length} - {FormatHelper.FormatTimeSpan(new TimeSpan(playlist.Sum(s => s.Tags?.TrackLength.Ticks ?? 1)))}";

        public AppSettingsViewModel SettingsViewModel { get; }

        public FullPlayerViewModel(AppSettings settings, SongFileFactory songFileFactory, SongPlayer player, Playlist playlist, PlayingVm playingVm, NextPrevVm nextPrevVm, Scrobbler scrobbler) {
            Settings = settings;
            this.playlist = playlist;
            SongPlayer = player;
            Playing = playingVm;
            NextPrev = nextPrevVm;
            Scrobbler = scrobbler;

            Playlist = new PlaylistVm(settings, songFileFactory, playlist, player, StartPlaying);
            SettingsViewModel = new AppSettingsViewModel(settings);
            Volume = new VolumeVm(settings);

            PlaySongCommand = new RelayCommand<Song>(StartPlaying, s => SongPlayer != null && s != null);

            LastfmLoginCmd = new AsyncCommand<(string usr, string pwd)>(t => Scrobbler.Login(t.usr, t.pwd), DefContinue);
            LastfmLogoutCmd = new RelayCommand(() => Scrobbler.Logout());

            playlist.CollectionChanged += (s, a) => {
                if(a.Action == NotifyCollectionChangedAction.Add || a.Action == NotifyCollectionChangedAction.Remove || a.Action == NotifyCollectionChangedAction.Reset)
                {
                    RaisePropertyChanged(nameof(ShowDropHint), nameof(PlaylistStats));
                }
            };
            Playlist.PropertyChanged += (s, a) => {
                if (a.PropertyName == nameof(Playlist.SelectedPlaylistItems))
                {
                    RaisePropertyChanged(nameof(PlaylistStats));
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

        private void DefContinue(Task t)
        {
            if (t.IsFaulted)
            {
                ExceptionWindow.Show(t.Exception.InnerException);
            }
        }
    }
}
