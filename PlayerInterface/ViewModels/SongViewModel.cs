﻿using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface.ViewModels {

    public class SongViewModel : NotifyPropertyChanged {
        private bool playing = false;
        public bool Playing {
            get { return playing; }
            set {
                if(playing != value) {
                    playing = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Song Song {
            get;
        }

        public string Path => $"{Song.FilePath}";

        public string SubTitle => $"{Song.Artist} {(string.IsNullOrEmpty(Song.Album) ? string.Empty : $"({Song.Album})")}";
        public string Title => $"{Song.Title}";

        public string TrackLengthStr => $"[{FormatHelper.FormatTimeSpan(Song.File.TrackLength)}]";
        public string BitRateStr => $"{Song.File.BitRate}kbps";
        public string TrackNrStr => Song.File.TrackCount == 0 ? $"#{Song.File.Track}" : $"{Song.File.Track}/{Song.File.TrackCount}";
        public string YearStr => Song.File.Year != 0 ? $"{Song.File.Year}" : "-";
        public string PlayCountStr => $"{Song.Stats.PlayCount}x";

        public enum DisplayType {
            Front, Menu, DropUnderHint
        }

        private DisplayType curDisplay;

        public DisplayType CurDisplay {
            get { return curDisplay; }
            set {
                if(value != curDisplay) {
                    curDisplay = value;
                    RaisePropertiesChanged(nameof(CurDisplay), nameof(FrontVisibility), nameof(MenuVisibility), nameof(DropUnderHintVisibility), nameof(MenuItems));
                }
            }
        }

        public Visibility FrontVisibility => CurDisplay == DisplayType.Front ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MenuVisibility => CurDisplay == DisplayType.Menu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DropUnderHintVisibility => CurDisplay == DisplayType.DropUnderHint ? Visibility.Visible : Visibility.Collapsed;

        public FullPlayerViewModel MainViewModel {
            get;
        }

        public static Dictionary<string, PropertyInfo> SortProperties;

        public IEnumerable<SongMenuItemViewModel> MenuItems => GetMenuItems();

        static SongViewModel() {
            var songtype = typeof(Song);
            var filetype = typeof(SongFile);
            var statType = typeof(PlayerCore.Settings.SongStats);
            SortProperties = new Dictionary<string, PropertyInfo>() {
                //Song:
                ["Title"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Title)),
                ["Album"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Album)),
                ["Artist"] = songtype.GetProperty(nameof(PlayerCore.Songs.Song.Artist)),
                //SongFile:
                ["Genre"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.Genre)),
                ["Length"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.TrackLength)),
                ["Year"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.Year)),
                ["Track #"] = filetype.GetProperty(nameof(PlayerCore.Songs.SongFile.Track)),
                //Stats
                ["Play Count"] = statType.GetProperty(nameof(PlayerCore.Settings.SongStats.PlayCount))
            };
        }

        /// <summary>
        /// For testing purposes only
        /// </summary>
        public SongViewModel() {
        }

        public SongViewModel(Song song, FullPlayerViewModel fpvm) {
            curDisplay = DisplayType.Front;
            Song = song;
            MainViewModel = fpvm;

            Song.Stats.PropertyChanged += (s, a) => RaisePropertiesChanged(nameof(PlayCountStr));
        }

        private IEnumerable<SongMenuItemViewModel> GetMenuItems() {
            if(CurDisplay != DisplayType.Menu) {
                yield break;
            }

            if(MainViewModel.SongPlayer.CurrentSong != Song) {
                yield return new SongMenuItemViewModel("Play", Play);
            }
            yield return new SongMenuItemViewModel("Remove", Remove);

            yield return new SongMenuItemViewModel("Open file location", OpenFileLocation);

            var foundStartup = MainViewModel.Settings.StartupFolders.FirstOrDefault(path => Path.StartsWith(path));
            if(foundStartup == null) {
                yield return new SongMenuItemViewModel("Add to startup songs", AddToStartup);
            } else {
                yield return new SongMenuItemViewModel("Remove from startup songs", RemoveFromStartup);
            }
        }

        private void OpenFileLocation() {
            if(File.Exists(Path)) {
                System.Diagnostics.Process.Start("explorer.exe", $"/select, {Path}");
            }
        }

        private void Play() {
            if(MainViewModel.PlaySongCommand.CanExecute(Song)) {
                MainViewModel.PlaySongCommand.Execute(Song);
            }
        }

        private void Remove() {
            var svmIEnum = new SongViewModel[] { this };
            if(MainViewModel.RemoveSongsCommand.CanExecute(svmIEnum)) {
                MainViewModel.RemoveSongsCommand.Execute(svmIEnum);
            }
        }

        private void AddToStartup() {
            MainViewModel.Settings.AddStartupFolder(Path);
            MainViewModel.SettingsViewModel.InitLoadPaths();
        }

        private void RemoveFromStartup() {
            Action<string, string> addAllExcept = null;
            addAllExcept = (add, except) => {
                if(File.Exists(add)) {
                    if(add != except) {
                        MainViewModel.Settings.AddStartupFolder(add);
                    }
                } else {
                    var di = new DirectoryInfo(add);
                    foreach(var addFile in di.GetFiles("*", SearchOption.TopDirectoryOnly).Where(fi => fi.FullName != Path)) {
                        MainViewModel.Settings.AddStartupFolder(addFile.FullName);
                    }
                    foreach(var addFolder in di.GetDirectories("*", SearchOption.TopDirectoryOnly)) {
                        if(!except.StartsWith(addFolder.FullName)) {
                            MainViewModel.Settings.AddStartupFolder(addFolder.FullName);
                        } else {
                            addAllExcept(addFolder.FullName, except);
                        }
                    }
                }
            };

            var existingPath = MainViewModel.Settings.StartupFolders.FirstOrDefault(path => Path.StartsWith(path));
            MainViewModel.Settings.RemoveStartupFolder(existingPath);
            addAllExcept(existingPath, Path);
            MainViewModel.SettingsViewModel.InitLoadPaths();
        }
    }
}