using System.Collections;
using System.Collections.Generic;
using VRage;
using System;

namespace RichHudFramework
{
    /// <summary>
    /// Interface for collections with an indexer and a count property.
    /// </summary>
    public interface IIndexedCollection<T>
    {
        /// <summary>
        /// Returns the element associated with the given index.
        /// </summary>
        T this[int index] { get; }

        /// <summary>
        /// The number of elements in the collection
        /// </summary>
        int Count { get; }
    }

    /// <summary>
    /// Read-only wrapper for types of <see cref="IDictionary{TKey, TValue}"/>
    /// </summary>
    public class ReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Returns the value associated with the given key
        /// </summary>
        public TValue this[TKey key] => dictionary[key];

        /// <summary>
        /// Returns the number of entries in the dictionary
        /// </summary>
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
    /// Read only wrapper for types of <see cref="IReadOnlyList{T}"/>
    /// </summary>
    public class ReadOnlyCollection<T> : IReadOnlyList<T>
    {
        public T this[int index] => collection[index];

        public int Count => collection.Count;

        protected readonly IReadOnlyList<T> collection;

        public ReadOnlyCollection(IReadOnlyList<T> collection)
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


    /// <summary>
    /// Generic enumerator using delegates.
    /// </summary>
    public class CollectionDataEnumerator<T> : IEnumerator<T>
    {
        /// <summary>
        /// Returns the element at the enumerator's current position
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Returns the element at the enumerator's current position
        /// </summary>
        public T Current => Getter(index);

        protected readonly Func<int, T> Getter;
        protected readonly Func<int> CountFunc;
        protected int index;

        public CollectionDataEnumerator(Func<int, T> Getter, Func<int> CountFunc)
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