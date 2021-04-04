using PlayerCore.Songs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore
{
    [DebuggerDisplay("{Length} Songs, current #{CurrentSongIndex})")]
    public class Playlist : IEnumerable<Song>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? CurrentSongChanged;
        public event EventHandler? QueueChanged;

        private readonly Random Random = new();

        private List<Song> Songs { get; set; }

        private int _CurrentIndex = -1;
        /// <summary>
        /// CurrentSong is the song at CurrentSongIndex in the Playlist.
        /// If index is set to a DIFFERENT value the CurrentSongChanged event will be raised
        /// Set to -1 for no song
        /// </summary>
        private int CurrentIndex {
            get => _CurrentIndex;
            set {
                if(value >= -1 && value < Length)
                {
                    if(_CurrentIndex != value)
                    {
                        _CurrentIndex = value;
                        RaiseCurrentSongChanged();
                    }
                }
            }
        }

        private List<Song> _Queue;
        public ReadOnlyCollection<Song> Queue => _Queue.AsReadOnly();

        private int? _QueueIndex;
        public int? QueueIndex {
            get => _QueueIndex;
            set {
                if(_QueueIndex != value)
                {
                    _QueueIndex = value;
                    RaiseCurrentSongChanged();
                }
            }
        }

        public Song? CurrentSong =>
            QueueIndex is int qi ? Queue[qi] :
            CurrentIndex >= 0 && CurrentIndex < Length ? Songs[CurrentIndex] :
                null;

        public Song this[int index] => Songs[index];

        public int Length => Songs.Count;

        public Playlist()
        {
            Songs = new List<Song>();
            _Queue = new List<Song>();
        }

        public IEnumerable<Song> AddSongs(IEnumerable<Song> songs, Song? after = null)
        {
            var ix = after != null ? Songs.IndexOf(after) : -1;
            ix = ix != -1 ? ix + 1 : Songs.Count;

            var addedSongs = new List<Song>();
            ChangeList(() => {
                var cIx = ix;
                foreach(var song in songs.Except(Songs, CompareSongByPath.Instance))
                {
                    Songs.Insert(cIx++, song);
                    addedSongs.Add(song);
                }
            });
            RaiseCollectionChanged(new(NotifyCollectionChangedAction.Add, addedSongs, ix));
            return addedSongs;
        }

        public void Enqueue(Song song)
        {
            if(!Queue.Contains(song))
            {
                _Queue.Add(song);
            }
            else
            {
                if(_Queue.IndexOf(song) is var oldIx && (oldIx == -1 || oldIx > QueueIndex || QueueIndex == null))
                {
                    var nxtIx = (QueueIndex ?? -1) + 1;
                    var nxt = nxtIx < _Queue.Count ? _Queue[nxtIx] : null;
                    if(nxt != song)
                    {
                        _Queue = _Queue.Select((c, ix) => (c, ix))
                            .OrderBy(t => t.c == song ? 0 : t.ix < nxtIx ? -1 : 1)
                            .Select(t => t.c)
                            .ToList();
                    }
                    else
                    {
                        _Queue.Remove(song);
                    }
                }
            }
            RaiseQueueChanged();
        }

        public void ClearQueue()
        {
            if(_Queue.Count > 0)
            {
                _Queue.Clear();
                RaiseQueueChanged();
            }
        }

        public void Shuffle(IEnumerable<Song>? source = null)
        {
            Order(s => Random.NextDouble(), source);
            if(Length > 0)
            {
                SetIndexForceUpdate(0);
            }
        }

        public void Order(Func<Song, object> orderBy, IEnumerable<Song>? source = null)
            => Order(new[] { orderBy }, source);

        public void Order(IEnumerable<Func<Song, object>> orderBys, IEnumerable<Song>? source = null)
        {
            ChangeList(() => {
                if(source is null)
                {
                    Songs = order(Songs).ToList();
                }
                else
                {
                    var sorted = order(source).ToArray();
                    var minIx = Songs.FindIndex(s => sorted.Contains(s));
                    for(int i = 0; i < sorted.Length; i++)
                    {
                        var newIx = minIx + i;
                        var oldIx = Songs.IndexOf(sorted[i]);
                        Songs.RemoveAt(oldIx);
                        newIx = newIx > oldIx ? newIx - 1 : newIx;
                        Songs.Insert(newIx, sorted[i]);
                    }
                }
            });
            RaiseCollectionChanged(new(NotifyCollectionChangedAction.Reset));

            IEnumerable<Song> order(IEnumerable<Song> inp)
            {
                if(orderBys.Any())
                {
                    var ioi = inp.OrderBy(orderBys.First());
                    foreach(var o in orderBys.Skip(1))
                    {
                        ioi = ioi.ThenBy(o);
                    }
                    return ioi;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot call {nameof(Order)} with emtpy {nameof(orderBys)} argument.");
                }
            }
        }

        public void Reverse()
        {
            ChangeList(Songs.Reverse);
            RaiseCollectionChanged(new(NotifyCollectionChangedAction.Reset));
        }

        public void SelectFirstMatch(Song song) => SelectFirstMatch((comp) => CompareSongByPath.Instance.Equals(comp, song));

        public void SelectFirstMatch(Predicate<Song> filter)
        {
            var index = Songs.FindIndex(filter);
            if(index >= 0)
            {
                SetIndexForceUpdate(index);
            }
        }

        public void MoveTo(Song destination, params Song[] source)
        {
            var index = destination == null ? 0 : Songs.IndexOf(destination) + 1;
            MoveToIndex(index, source);
        }

        public void MoveToIndex(int index, params Song[] source)
        {
            ChangeList(() => {
                for(int i = source.Length - 1; i >= 0; i--)
                {
                    var move = source[i];
                    var orgIx = Songs.IndexOf(move);
                    var newIx = index;
                    if(newIx > orgIx) {
                        newIx--;
                        index--;
                    }
                    Songs.RemoveAt(orgIx);
                    Songs.Insert(newIx, move);
                    RaiseCollectionChanged(new(NotifyCollectionChangedAction.Move, move, newIx, orgIx));
                }
            });
        }

        public void Remove(IEnumerable<Song> songs)
        {
            foreach(var song in songs.ToArray())
            {
                Remove(song);
            }
        }

        public void Remove(Song song)
        {
            if(_Queue.Remove(song))
            {
                RaiseQueueChanged();
            }

            ChangeList(() => {
                Songs.Remove(song);
            });
            RaiseCollectionChanged(new(NotifyCollectionChangedAction.Remove, song));
        }

        public bool HasNext(bool loop) => GetDoNext(loop) != null;
        public bool Next(bool loop)
        {
            var doNext = GetDoNext(loop);
            if(doNext != null)
            {
                doNext();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool HasPrevious(bool loop) => GetDoPrevious(loop) != null;
        public bool Previous(bool loop)
        {
            var doPrev = GetDoPrevious(loop);
            if(doPrev != null)
            {
                doPrev();
                return true;
            }
            else
            {
                return false;
            }
        }

        private Action? GetDoNext(bool loop)
        {
            Action? GetUpdateCurrentIndexAction()
            {
                if(Songs.Count > 1)
                {
                    if(CurrentIndex + 1 < Songs.Count)
                    {
                        return () => CurrentIndex++;
                    }
                    else if(loop && CurrentIndex + 1 == Songs.Count)
                    {
                        return () => CurrentIndex = 0;
                    }
                }
                return null;
            }

            if(QueueIndex.HasValue)
            {
                if(QueueIndex + 1 < _Queue.Count)
                {
                    return () => QueueIndex++;
                }
                else
                {
                    return () => {
                        QueueIndex = null;
                        ClearQueue();
                    };
                }
            }
            else
            {
                if(_Queue.Count == 0)
                {
                    return GetUpdateCurrentIndexAction();
                }
                else
                {
                    return () => {
                        GetUpdateCurrentIndexAction()?.Invoke();
                        QueueIndex = 0;
                    };
                }
            }
        }

        private Action? GetDoPrevious(bool loop)
        {
            if(QueueIndex.HasValue)
            {
                if(QueueIndex > 0)
                {
                    return () => QueueIndex--;
                }
                else
                {
                    return () => QueueIndex = null;
                }
            }
            else
            {
                if(Songs.Count > 1)
                {
                    if(CurrentIndex > 0)
                    {
                        return () => CurrentIndex--;
                    }
                    else if(loop && CurrentIndex == 0)
                    {
                        return () => CurrentIndex = Songs.Count - 1;
                    }
                }
            }
            return null;
        }

        private void ChangeList(Action change)
        {
            var orgIndex = CurrentIndex;
            var orgLenght = Length;
            var orgSong = CurrentSong;
            change();

            var foundIndex = orgSong != null ? Songs.FindIndex((s) => s == orgSong) : -1;
            if(foundIndex > -1)
            {
                _CurrentIndex = foundIndex;
            }
            else
            {
                var newIndex = (orgIndex - (orgLenght - Length)) + 1;
                newIndex = newIndex < Length ? newIndex : (Length > 0 ? Length - 1 : -1);
                SetIndexForceUpdate(newIndex);
            }
        }

        /// <summary>
        /// Changes CurrentSongIndex, always raises CurrentSongChanged event, even if index stays the same.
        /// This is usefull when the list has changed since the last time the index was set and a new song is at the current index.
        /// </summary>
        /// <param name="newIndex">Nr to set CurrentSongIndex to, -1 for no song.</param>
        private void SetIndexForceUpdate(int newIndex)
        {
            _CurrentIndex = (_CurrentIndex == -1) ? -2 : -1;
            CurrentIndex = newIndex;

            QueueIndex = null;
        }

        private void RaiseCurrentSongChanged() => CurrentSongChanged?.Invoke(this, EventArgs.Empty);
        private void RaiseQueueChanged() => QueueChanged?.Invoke(this, EventArgs.Empty);

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IEnumerator<Song> GetEnumerator() => Songs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
