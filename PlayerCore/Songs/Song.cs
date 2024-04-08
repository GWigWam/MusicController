using PlayerCore.Persist;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.Songs
{
    [DebuggerDisplay("'{Title}' ({Tags})")]
    public record Song : IEqualityComparer<Song>, IEquatable<Song>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Path { get; }
        public string Title { get; private set; }

        private SongTags? _Tags;
        public SongTags? Tags {
            get => _Tags;
            set {
                _Tags = value;
                PropertyChanged?.Invoke(this, new(nameof(Tags)));
                if (value?.Title != null) {
                    Title = value.Title;
                    PropertyChanged?.Invoke(this, new(nameof(Title)));
                }
            }
        }

        private SongStats? _Stats;
        public SongStats? Stats {
            get => _Stats;
            set {
                _Stats = value;
                PropertyChanged?.Invoke(this, new(nameof(Stats)));
            }
        }

        internal Song(string path, string title, SongTags? tags = null, SongStats? stats = null)
        {
            Path = path;
            Title = title;
            _Tags = tags;
            _Stats = stats;
        }

        public virtual bool Equals(Song? other) => Equals(this, other);
        public bool Equals(Song? x, Song? y) => x?.GetHashCode() == y?.GetHashCode();
        public int GetHashCode(Song obj) => obj.GetHashCode();
        public override int GetHashCode() => Path.ToLower().GetHashCode();
    }
}
