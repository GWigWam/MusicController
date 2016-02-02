using PlayerInterface.CustomControls;
using PlayerInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlayerInterface {

    /// <summary>
    /// Interaction logic for FullPlayer.xaml
    /// </summary>
    public partial class FullPlayer : Window {

        public event EventHandler MinimizedToTray;

        private bool SlidingElapsed = false;

        private FullPlayerViewModel Model => DataContext as FullPlayerViewModel;

        public FullPlayer(FullPlayerViewModel fpvm) {
            InitializeComponent();

            DataContext = fpvm;
            fpvm.PropertyChanged += (s, p) => {
                if(p.PropertyName == nameof(FullPlayerViewModel.CurrentFocusItem)) {
                    var item = Lb_Playlist.Items.Cast<SongViewModel>().FirstOrDefault(svm => svm.Playing);
                    if(item != null) {
                        Lb_Playlist.ScrollIntoView(item);
                    }
                }
            };
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e) {
            if(e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Btn_Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Application.Current.Shutdown();
        }

        private void Btn_Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            MinimizeToTray();
        }

        public void MinimizeToTray() {
            Hide();
            MinimizedToTray?.Invoke(this, new EventArgs());
        }

        private void Slr_Elapsed_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if(e.LeftButton == MouseButtonState.Pressed) {
                if(Model != null) {
                    if(Model.SongPlayer.PlayerState == NAudio.Wave.PlaybackState.Playing) {
                        Model.SongPlayer.PlayerState = NAudio.Wave.PlaybackState.Paused;
                        SlidingElapsed = true;
                    }
                }
            }
        }

        private void Slr_Elapsed_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            if(SlidingElapsed) {
                if(Model != null) {
                    Model.SongPlayer.PlayerState = NAudio.Wave.PlaybackState.Playing;
                    SlidingElapsed = false;
                }
            }
        }

        private void Lb_Playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selected = Lb_Playlist.SelectedItem as SongViewModel;
            if(Model?.PlaySongCommand != null && selected != null) {
                if(Model.PlaySongCommand.CanExecute(selected.Song)) {
                    Model.PlaySongCommand.Execute(selected.Song);
                }
            }
        }

        private void Btn_Sort_MouseUp(object sender, MouseButtonEventArgs e) {
            var btn = sender as ImageButton;
            if(e.ChangedButton == MouseButton.Left && btn != null) {
                var cm = btn.ContextMenu;
                cm.PlacementTarget = btn;
                cm.IsOpen = true;
            }
        }

        private void Btn_Sort_Loaded(object sender, RoutedEventArgs e) {
            var cm = (sender as ImageButton)?.ContextMenu;
            if(cm != null) {
                foreach(var prop in SongViewModel.SortProperties) {
                    var mi = new MenuItem() {
                        Header = prop.Key,
                        Command = Model.SortByCommand,
                        CommandParameter = prop.Value
                    };
                    cm.Items.Add(mi);
                }
            }
        }

        private void Lb_Playlist_KeyUp(object sender, KeyEventArgs e) {
            var singleSelected = Lb_Playlist.SelectedItem as SongViewModel;
            var allSelected = Lb_Playlist.SelectedItems.Cast<SongViewModel>();

            if(e.Key == Key.Delete) {
                if(Model?.RemoveSongCommand != null && allSelected.Count() > 0) {
                    if(Model.RemoveSongCommand.CanExecute(allSelected)) {
                        Model.RemoveSongCommand.Execute(allSelected);
                    }
                }
            } else if(e.Key == Key.Enter) {
                if(Model?.PlaySongCommand != null && singleSelected != null) {
                    if(Model.PlaySongCommand.CanExecute(singleSelected.Song)) {
                        Model.PlaySongCommand.Execute(singleSelected.Song);
                    }
                }
            }
        }
    }
}