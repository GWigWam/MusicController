using Newtonsoft.Json;
using PlayerCore.Songs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PlayerCore.Persist
{
    public class FileTagsCache : PersistToFile
    {
        [JsonProperty]
        private Dictionary<string, SongTags> Cache { get; } = new();

        public FileTagsCache(string filePath) : base(filePath) { }

        [JsonConstructor]
        public FileTagsCache() : base() { }

        public bool TryGet(string path, [MaybeNullWhen(false)]out SongTags? tags)
            => Cache.TryGetValue(path.ToLower(), out tags);

        public void AddOrUpdate(string path, SongTags tags)
        {
            Cache[path.ToLower()] = tags;
            RaiseChanged(nameof(Cache));
        }

        public bool Remove(string path) => Cache.Remove(path.ToLower());
    }
}
