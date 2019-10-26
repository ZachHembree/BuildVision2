using System.Collections;
using System.Collections.Generic;
using VRage;
using System;

namespace DarkHelmet
{
    public interface IIndexedCollection<T>
    {
        T this[int index] { get; }
        int Count { get; }
    }

    public interface IReadOnlyCollection<T> : IIndexedCollection<T>, IEnumerable<T>
    { }

    public class ReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public TValue this[TKey key] => dictionary[key];
        public int Count => dictionary.Count;

        protected readonly IDictionary<TKey, TValue> dictionary;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    /// <summary>
    /// Read only wrapper for types of <see cref="IList{T}"/> in lieu of the one in System.Collections.ObjectModel. 
    /// The indexer doesn't allow modification of the collection, but if the underlying collection is modified, this will reflect that.
    /// </summary>
    public class ReadOnlyCollection<T> : IReadOnlyCollection<T>
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

    public class ReadOnlyCollectionData<T> : IReadOnlyCollection<T>
    {
        public T this[int index] => Getter(index);
        public int Count => CountFunc();

        private readonly Func<int, T> Getter;
        private readonly Func<int> CountFunc;
        private readonly IEnumerator<T> enumerator;

        public ReadOnlyCollectionData(Func<int, T> Getter, Func<int> CountFunc)
        {
            this.Getter = Getter;
            this.CountFunc = CountFunc;
            enumerator = new CollectionEnumeratorData<T>(Getter, CountFunc);
        }

        public IEnumerator<T> GetEnumerator() =>
            enumerator;

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

    public class CollectionEnumeratorData<T> : IEnumerator<T>
    {
        object IEnumerator.Current => Current;
        public T Current => Getter(index);

        private readonly Func<int, T> Getter;
        private readonly Func<int> CountFunc;
        private int index;

        public CollectionEnumeratorData(Func<int, T> Getter, Func<int> CountFunc)
        {
            this.Getter = Getter;
            this.CountFunc = CountFunc;
            index = -1;
        }

        public void Dispose()
        { }

        public bool MoveNext()
        {
            index++;
            return index < CountFunc();
        }

        public void Reset()
        {
            index = -1;
        }
    }
}