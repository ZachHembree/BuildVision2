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
    /// An indexed, enumerable read-only collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyCollection<T> : IIndexedCollection<T>, IEnumerable<T>
    { }

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
}