using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PlayerInterface
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                collection.Add(item);
            }
        }

        public static void InsertRange<T>(this ObservableCollection<T> collection, int index, IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                collection.Insert(index++, item);
            }
        }

        public static void RemoveRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                collection.Remove(item);
            }
        }
    }
}
