﻿using PlayerCore;
using PlayerInterface.CustomControls;
using PlayerInterface.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlayerInterface {
    public partial class FullPlayer : Window {

        public event EventHandler MinimizedToTray;

        private FullPlayerViewModel Vm => (FullPlayerViewModel)DataContext;

        private ScrollViewer lb_Playlist_ScrollViewer;

        private ScrollViewer Lb_Playlist_ScrollViewer {
            get {
                if(lb_Playlist_ScrollViewer == null) {
                    lb_Playlist_ScrollViewer = GetScrollViewer(Lb_Playlist);
                }
                return lb_Playlist_ScrollViewer;
            }
        }

        /// <summary>
        /// SongCards to be used in next drag action
        /// </summary>
        private SongViewModel[] ItemsToDrag { get; set; }

        public FullPlayer(FullPlayerViewModel fpvm) {
            InitializeComponent();
            DataContext = fpvm;
            
            fpvm.PropertyChanged += (s, p) => {
                if(p.PropertyName == nameof(FullPlayerViewModel.StatusText)) {
                    UpdateStatusTextAnimation();
                }
            };
            fpvm.Playlist.DisplayedSongsChanged += (s, a) => Application.Current.Dispatcher.Invoke(ScrollCurrentSongIntoView);
            fpvm.SongPlayer.SongChanged += (s, a) => Application.Current.Dispatcher.Invoke(ScrollCurrentSongIntoView);
            
            IsVisibleChanged += (s, a) => {
                if (IsVisible) {
                    Application.Current.Dispatcher.Invoke(ScrollCurrentSongIntoView);
                }
            };

            SizeChanged += (s, a) => UpdateStatusTextAnimation();

            setupCurPosRect();

            void setupCurPosRect()
            {
                updateCurPosRect();
                fpvm.Playlist.DisplayedSongsChanged += (s, a) => Application.Current.Dispatcher.BeginInvoke(new Action(updateCurPosRect));
                fpvm.SongPlayer.SongChanged += (s, a) => Application.Current.Dispatcher.BeginInvoke(new Action(updateCurPosRect));

                void updateCurPosRect()
                {
                    const int topMargin = 20;

                    var playing = Lb_Playlist.Items.OfType<SongViewModel>().FirstOrDefault(svm => svm.Playing);
                    if(Lb_Playlist.Items.Count > 12)
                    {
                        var ix = Lb_Playlist.Items.IndexOf(playing);
                        var frac = (double)ix / Lb_Playlist.Items.Count;
                        var scrollHeight = Lb_Playlist.ActualHeight - (2 * topMargin);

                        var y = (scrollHeight * frac) - (Rect_CurPos.Height / 2) + topMargin;
                        Canvas.SetTop(Rect_CurPos, y);
                        Rect_CurPos.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Rect_CurPos.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void MinimizeToTray() {
            Hide();
            MinimizedToTray?.Invoke(this, new EventArgs());
        }

        private void ScrollCurrentSongIntoView() {
            var playing = Lb_Playlist.Items.Cast<SongViewModel>().FirstOrDefault(svm => svm.Playing);
            if(playing != null) {
                Lb_Playlist.ScrollIntoView(playing);
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject obj) {
            if(obj is ScrollViewer)
                return obj as ScrollViewer;

            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
                var child = VisualTreeHelper.GetChild(obj, i);

                var result = GetScrollViewer(child);
                if(result != null) {
                    return result;
                }
            }
            return null;
        }

        private void UpdateStatusTextAnimation() {
            // Delay to make sure the 'ActualWidht' property has updated
            // Yes this is a hack, however both OnInitialize and OnLoaded are called before 'ActualWidth' is set so it's hard to act at the right moment
            Task.Delay(500).ContinueWith((t) =>
                Dispatcher.BeginInvoke(new Action(() => {
                    var sb = Grid_StatusText.Resources["Sb_StatusText"] as Storyboard;
                    sb.Stop(this);
                    sb.Begin(this, true);
                }))
            );
        }

        private void Btn_Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Application.Current.Shutdown();
        }

        private void Sp_Drag_MouseDown(object sender, MouseButtonEventArgs e) {
            if(e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Btn_Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            MinimizeToTray();
        }

        private void Lb_Playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selected = Lb_Playlist.SelectedItem as SongViewModel;
            if(Vm?.PlaySongCommand != null && selected != null && e.ChangedButton == MouseButton.Left) {
                if(Vm.PlaySongCommand.CanExecute(selected.Song)) {
                    Vm.PlaySongCommand.Execute(selected.Song);
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
                        Command = Vm.Playlist.SortByCommand,
                        CommandParameter = prop.Value
                    };
                    cm.Items.Add(mi);
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
                if(e.Key == Key.Q) {
                    var sel = Vm.Playlist.SelectedPlaylistItems.Select(svm => svm.Song).ToArray();
                    if (sel.Any())
                    {
                        Vm.Playlist.Enqueue(sel);
                    }
                }
            } else {
                if(!Tb_Search.IsFocused && e.Key > Key.A && e.Key < Key.Z) {
                    Tb_Search.Focus();
                } else if((!Tb_Search.IsFocused && e.Key == Key.Back) || e.Key == Key.Escape) {
                    Tb_Search.Text = string.Empty;
                } else if(!Tb_Search.IsFocused && e.Key == Key.Space) {
                    var switchCmd = Vm?.Playing?.SwitchCommand;
                    if(switchCmd?.CanExecute(null) == true) {
                        switchCmd.Execute(null);
                    }
                }
            }
        }

        private void Lb_Playlist_KeyUp(object sender, KeyEventArgs e) {
            var singleSelected = Lb_Playlist.SelectedItem as SongViewModel;
            var allSelected = Lb_Playlist.SelectedItems.Cast<SongViewModel>();

            if(e.Key == Key.Delete) {
                if(allSelected.Count() > 0) {
                    if(Vm.Playlist.RemoveSongsCommand.CanExecute(allSelected)) {
                        Vm.Playlist.RemoveSongsCommand.Execute(allSelected);
                    }
                }
            } else if(e.Key == Key.Enter) {
                if(Vm?.PlaySongCommand != null && singleSelected != null) {
                    if(Vm.PlaySongCommand.CanExecute(singleSelected.Song)) {
                        Vm.PlaySongCommand.Execute(singleSelected.Song);
                    }
                }
            }
        }

        private void Lb_Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach(var svm in e.AddedItems.OfType<SongViewModel>()) {
                svm.IsSelected = true;
            }
            foreach(var svm in e.RemovedItems.OfType<SongViewModel>()) {
                svm.IsSelected = false;
            }
        }

        private void Tb_Search_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            ((TextBox)sender).SelectAll();
        }

        private Point? DragLeftClickPoint = null;
        private void SongCard_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                if(sender is ListBoxItem item && item.IsSelected && (ItemsToDrag?.Length ?? -1) > 0)
                {
                    var mPos = e.GetPosition(this);
                    if(DragLeftClickPoint == null)
                    {
                        DragLeftClickPoint = mPos;
                    }
                    else if((mPos - DragLeftClickPoint.Value).Length > 20) // Prevent accidental drag by enforcing minimum drag distance
                    {
                        DragDrop.DoDragDrop(item, ItemsToDrag, DragDropEffects.Move);
                    }
                }
            }
            else
            {
                DragLeftClickPoint = null;
            }
        }

        /// <summary>
        /// When draging over Lb_playlist scorl up/down when dragged item (mouse) is near the bottum/top of list
        /// </summary>
        private void SongCard_DragOver(object sender, DragEventArgs e) {
            const int maxScrollSpeed = 10;

            if(Lb_Playlist_ScrollViewer != null) {
                var yPos = e.GetPosition(Lb_Playlist).Y;
                var height = Lb_Playlist.ActualHeight;
                var minDistToEdge = (int)(height / 5);
                var curScrollOffset = Lb_Playlist_ScrollViewer.VerticalOffset;

                if(yPos < minDistToEdge) { //Scroll up
                    var scrollSpeed = Math.Ceiling(maxScrollSpeed * ((minDistToEdge - yPos) / minDistToEdge));
                    Lb_Playlist_ScrollViewer.ScrollToVerticalOffset(curScrollOffset - scrollSpeed);
                } else if(yPos > height - minDistToEdge) { //Scroll down
                    var scrollSpeed = Math.Ceiling(maxScrollSpeed * (((height - minDistToEdge) - yPos) / minDistToEdge));
                    Lb_Playlist_ScrollViewer.ScrollToVerticalOffset(curScrollOffset - scrollSpeed);
                }
            }
        }

        private void SongCard_DragEnter(object sender, DragEventArgs e) {
            if (sender is ListBoxItem lbi && lbi.DataContext is SongViewModel svm) {
                svm.CurDisplay = SongViewModel.DisplayType.DropUnderHint;
            }
        }

        private void SongCard_DragLeave(object sender, DragEventArgs e) {
            if(sender is ListBoxItem lbi && lbi.DataContext is SongViewModel svm) {
                svm.CurDisplay = SongViewModel.DisplayType.Front;
            }
        }

        private void SongCard_Drop(object sender, DragEventArgs e) {
            if (sender is ListBoxItem lbi && lbi.DataContext is SongViewModel droppedOn) {
                if(e.Data.GetDataPresent(typeof(SongViewModel[]))) {
                    var data = e.Data.GetData(typeof(SongViewModel[])) as SongViewModel[];

                    if(data != null && !data.Contains(droppedOn)) {
                        var args = (data, droppedOn);
                        if (Vm.Playlist.MovePlaylistSongsCommand.CanExecute(args)) {
                            Vm.Playlist.MovePlaylistSongsCommand.Execute(args);
                        }
                        e.Handled = true;
                    }
                } else if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
                    if(Vm.Playlist.AddFilesCommand != null) {
                        var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                        var args = new { Paths = paths, Position = droppedOn.Song };
                        if(Vm.Playlist.AddFilesCommand.CanExecute(args)) {
                            Vm.Playlist.AddFilesCommand.Execute(args);
                            e.Handled = true;
                        }
                    }
                }
                droppedOn.CurDisplay = SongViewModel.DisplayType.Front;
            }
        }

        private void SongCard_PreviewMouseDown(object sender, MouseEventArgs e) {
            var nrOfItems = Lb_Playlist.SelectedItems.Count;
            ItemsToDrag = new SongViewModel[nrOfItems];
            Lb_Playlist.SelectedItems.CopyTo(ItemsToDrag, 0);
        }

        private void SongCard_MouseUp(object sender, MouseEventArgs e) {
            ItemsToDrag = null;

            if(e is MouseButtonEventArgs mbea && mbea.ChangedButton == MouseButton.Right)
            {
                if(sender is ListBoxItem lbi && lbi.DataContext is SongViewModel svm)
                {
                    svm.IsMenuVisible = true;
                }
            }
        }

        private void Lb_Playlist_Drop(object sender, DragEventArgs e) {
            if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                var args = new { Paths = paths, Position = (int?)null };
                if(Vm.Playlist.AddFilesCommand.CanExecute(args)) {
                    Vm.Playlist.AddFilesCommand.Execute(args);
                }
            }
        }

        private void Tab_MouseUp(object sender, MouseButtonEventArgs e) {
            if(e.ChangedButton == MouseButton.Left) {
                var currentTab = (sender as FrameworkElement)?.Parent as Border;
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

        private void Border_SongCard_Menu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if(sender is FrameworkElement fe && fe.DataContext is SongMenuItemViewModel smivm && fe.Tag is SongViewModel svm) {
                svm.IsMenuVisible = false;
                smivm.Execute();
                e.Handled = true;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Tb_Search_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                Vm.Playlist.PlayTopResultCommand.Execute(null);
                e.Handled = true;
            } else if(e.Key == Key.Q && e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
                Vm.Playlist.QueueTopResultCommand.Execute(null);
                e.Handled = true;
            } else if(e.Key == Key.Space && Tb_Search.Text.Length == 0) {
                if(Vm.Playing.SwitchCommand.CanExecute(null)) {
                    Vm.Playing.SwitchCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void Lb_Playlist_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            ScrollViewer _getScroller(DependencyObject elem) {
                if(elem is ScrollViewer sv) {
                    return sv;
                }
                for(int i = 0; i < VisualTreeHelper.GetChildrenCount(elem); i++) {
                    var ch = VisualTreeHelper.GetChild(elem, i);
                    var r = _getScroller(ch);
                    if(r != null) {
                        return r;
                    }
                }
                return null;
            }

            if(e.Delta != 0 && e.LeftButton == MouseButtonState.Released &&e.RightButton == MouseButtonState.Released && sender is ListBox lb) {
                var sv = _getScroller(lb);
                var curOff = sv.VerticalOffset;
                var nextOff = e.Delta < 0 ? (curOff + 1) : (curOff > 0 ? curOff - 1 : 0);
                sv.ScrollToVerticalOffset(nextOff);
                e.Handled = true;
            }
        }

        private void Bt_LastfmLogin_Click(object sender, RoutedEventArgs e)
        {
            if (Pwd_Lastfm.Password.Length > 0)
            {
                var param = (Vm.Settings.LastfmUser, Pwd_Lastfm.Password);
                if (Vm.LastfmLoginCmd.CanExecute(param))
                {
                    Vm.LastfmLoginCmd.Execute(param);
                }
            }
        }
    }
}
