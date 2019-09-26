using System.Collections;
using System.Collections.Generic;

namespace DarkHelmet
{
    public interface IIndexedCollection<T>
    {
        T this[int index] { get; }
        int Count { get; }
    }

    public interface IIndexedEnumerable<T> : IIndexedCollection<T>, IEnumerable<T>
    { }

    /// <summary>
    /// Read only wrapper for types of <see cref="IList{T}"/> in lieu of the one in System.Collections.ObjectModel. 
    /// The indexer doesn't allow modification of the collection, but if the underlying collection is modified, this will reflect that.
    /// </summary>
    public class ReadOnlyCollection<T> : IIndexedEnumerable<T>
    {
        public T this[int index] => collection[index];
        public int Count => collection.Count;

        protected readonly IList<T> collection;

        public ReadOnlyCollection(IList<T> collection)
        {
            this.collection = collection;
        }

        protected ReadOnlyCollection(int capacity = 3)
        {
            collection = new List<T>(capacity);
        }

        public IEnumerator<T> GetEnumerator() =>
            collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    public class CollectionEnumerator<T> : IEnumerator<T>
    {
        object IEnumerator.Current => Current;
        public T Current => collection[index];

        private readonly IList<T> collection;
        private int index;

        public CollectionEnumerator(IList<T> collection)
        {
            this.collection = collection;
            index = -1;
        }

        public void Dispose()
        { }

        public bool MoveNext()
        {
            index++;
            return index < collection.Count;
        }

        public void Reset()
        {
            index = -1;
        }
    }
}