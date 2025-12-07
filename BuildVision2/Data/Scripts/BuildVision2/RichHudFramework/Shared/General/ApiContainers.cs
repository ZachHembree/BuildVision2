using System;
using System.Collections;
using System.Collections.Generic;
using VRage;

namespace RichHudFramework
{
	/// <summary>
	/// Read-only collection that lazily caches API wrappers on-demand while exposing an up-to-date count.
	/// </summary>
	/// <typeparam name="TValue">Type of the API wrapper</typeparam>
	/// <exclude/>
	public class ReadOnlyApiCollection<TValue> : IReadOnlyList<TValue>, IIndexedCollection<TValue>
	{
		/// <summary>
		/// Retrieves the wrapper at the specified index. Creates and caches new wrappers as needed.
		/// </summary>
		/// <exception cref="Exception">Thrown if the index is negative or >= <see cref="Count"/>.</exception>
		public virtual TValue this[int index]
		{
			get
			{
				int count = GetCountFunc();

				if (index >= count)
					throw new Exception($"Index ({index}) was out of Range. Must be non-negative and less than {count}.");

				while (wrapperList.Count < count)
				{
					for (int n = wrapperList.Count; wrapperList.Count < count; n++)
						wrapperList.Add(GetNewWrapperFunc(n));
				}

				if (count > 9 && wrapperList.Count > count * 3)
				{
					wrapperList.RemoveRange(count, wrapperList.Count - count);
					wrapperList.TrimExcess();
				}

				return wrapperList[index];
			}
		}

		/// <summary>
		/// Number of elements currently in the underlying collection.
		/// </summary>
		public virtual int Count => GetCountFunc();

		protected readonly Func<int, TValue> GetNewWrapperFunc;
		protected readonly Func<int> GetCountFunc;
		protected readonly List<TValue> wrapperList;
		protected readonly CollectionDataEnumerator<TValue> enumerator;

		/// <summary>
		/// Constructs a new lazily-cached read-only collection using the provided factory delegates.
		/// </summary>
		public ReadOnlyApiCollection(Func<int, TValue> getNewWrapper, Func<int> getCount)
		{
			this.GetNewWrapperFunc = getNewWrapper;
			this.GetCountFunc = getCount;

			wrapperList = new List<TValue>();
			enumerator = new CollectionDataEnumerator<TValue>(i => this[i], getCount);
		}

		public ReadOnlyApiCollection(MyTuple<Func<int, TValue>, Func<int>> tuple)
			: this(tuple.Item1, tuple.Item2)
		{ }

		public virtual IEnumerator<TValue> GetEnumerator() => enumerator;
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/// <summary>
	/// Lightweight read-only collection backed entirely by delegates. No internal caching.
	/// </summary>
	/// <typeparam name="TValue">Type of element returned by the collection</typeparam>
	/// <exclude/>
	public class ReadOnlyCollectionData<TValue> : IReadOnlyList<TValue>, IIndexedCollection<TValue>
	{
		/// <summary>
		/// Retrieves the element at the specified index via the getter delegate.
		/// </summary>
		public virtual TValue this[int index] => Getter(index);

		/// <summary>
		/// Current number of elements reported by the underlying source.
		/// </summary>
		public virtual int Count => GetCountFunc();

		protected readonly Func<int, TValue> Getter;
		protected readonly Func<int> GetCountFunc;
		protected readonly CollectionDataEnumerator<TValue> enumerator;

		public ReadOnlyCollectionData(Func<int, TValue> getter, Func<int> getCount)
		{
			this.Getter = getter;
			this.GetCountFunc = getCount;
			enumerator = new CollectionDataEnumerator<TValue>(i => this[i], getCount);
		}

		public ReadOnlyCollectionData(MyTuple<Func<int, TValue>, Func<int>> tuple)
			: this(tuple.Item1, tuple.Item2)
		{ }

		public virtual IEnumerator<TValue> GetEnumerator() => enumerator;
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}