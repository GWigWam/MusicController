using PlayerCore;
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
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => ScrollCurrentSongIntoView()));
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
                    if(Model.SongPlayer.PlayerState == PlayerState.Playing) {
                        Model.SongPlayer.PlayerState = PlayerState.Paused;
                        SlidingElapsed = true;
                    }
                }
            }
        }

        private void Slr_Elapsed_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            if(SlidingElapsed) {
                if(Model != null) {
                    Model.SongPlayer.PlayerState = PlayerState.Playing;
                    SlidingElapsed = false;
                }
            }
        }

        private void Lb_Playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selected = Lb_Playlist.SelectedItem as SongViewModel;
            if(Model?.PlaySongCommand != null && selected != null && e.ChangedButton == MouseButton.Left) {
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

        private void Tb_Search_TextChanged(object sender, TextChangedEventArgs e) {
            var input = (sender as TextBox)?.Text;
            if(input != null && Model?.SearchCommand != null && Model.SearchCommand.CanExecute(input)) {
                Model.SearchCommand.Execute(input);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(!Tb_Search.IsFocused && e.Key > Key.A && e.Key < Key.Z) {
                Tb_Search.Focus();
            } else if((!Tb_Search.IsFocused && e.Key == Key.Back) || e.Key == Key.Escape) {
                Tb_Search.Text = string.Empty;
            } else if(!Tb_Search.IsFocused && e.Key == Key.Space) {
                if(Model?.SwitchCommand?.CanExecute(null) == true) {
                    Model.SwitchCommand.Execute(null);
                }
            }
        }

        private void Tb_Search_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            ((TextBox)sender).SelectAll();
        }

        private void Lb_Playlist_Drop(object sender, DragEventArgs e) {
            if(Model?.AddFilesCommand != null && e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if(Model.AddFilesCommand.CanExecute(paths)) {
                    Model.AddFilesCommand.Execute(paths);
                }
            }
        }

        private void Tab_MouseUp(object sender, MouseButtonEventArgs e) {
            if(e.ChangedButton == MouseButton.Left) {
                var currentTab = (sender as Image)?.Parent as Border;
                var currentTabName = currentTab.Name.Replace("Btn_", "");
                if(currentTab != null) {
                    foreach(var tab in Sp_Tabs.Children.Cast<Border>()) {
                        tab.Tag = "InActive";
                    }

                    foreach(var grid in Grid_Tabs.Children.Cast<Grid>()) {
                        if(grid.Name.EndsWith(currentTabName)) {
                            grid.Visibility = Visibility.Visible;
                        } else {
                            grid.Visibility = Visibility.Collapsed;
                        }
                    }

                    currentTab.Tag = "Active";
                }
            }
        }

        private void TreeView_LostFocus(object sender, RoutedEventArgs e) {
            var tv = sender as TreeView;
            if(!tv.IsFocused && !tv.IsKeyboardFocusWithin) {
                Model.SettingsViewModel.TreeView_LostFocus(sender, e);
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if((bool)e.NewValue == true) {
                ScrollCurrentSongIntoView();
            }
        }

        private void ScrollCurrentSongIntoView() {
            var item = Lb_Playlist.Items.Cast<SongViewModel>().FirstOrDefault(svm => svm.Playing);
            if(item != null) {
                Lb_Playlist.ScrollIntoView(item);
            }
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var svm = ((sender as Grid).DataContext as SongViewModel);
            if(svm != null) {
                svm.MenuActive = !svm.MenuActive;
            }
        }
    }
}