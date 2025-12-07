using System;
using System.Collections;
using System.Collections.Generic;

namespace RichHudFramework
{
	/// <summary>
	/// Minimal interface for any indexed collection exposing a count and an indexer.
	/// </summary>
	public interface IIndexedCollection<T>
	{
		/// <summary>
		/// Retrieves the element at the specified index.
		/// </summary>
		T this[int index] { get; }

		/// <summary>
		/// Number of elements in the collection.
		/// </summary>
		int Count { get; }
	}

	/// <summary>
	/// Lightweight enumerator that satisfies <see cref="IEnumerable{T}"/> using only delegates.
	/// <para>
	/// Not ideal for the SE mod profiler, but it's an easy way to satisfy interface constraints.
	/// </para>
	/// </summary>
	/// <exclude/>
	public class CollectionDataEnumerator<T> : IEnumerator<T>
	{
		/// <summary>
		/// Current element pointed to by the enumerator.
		/// </summary>
		public T Current => Getter(index);

		/// <summary>
		/// Current element as <see cref="object"/> for non-generic <see cref="IEnumerator"/> compatibility.
		/// </summary>
		object IEnumerator.Current => Current;

		private readonly Func<int, T> Getter;
		private readonly Func<int> CountFunc;
		private int index = -1;

		public CollectionDataEnumerator(Func<int, T> getter, Func<int> countFunc)
		{
			Getter = getter;
			CountFunc = countFunc;
		}

		/// <summary>
		/// Advances the enumerator to the next element.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced; false if it has passed the end of the collection.</returns>
		public bool MoveNext()
		{
			index++;
			return index < CountFunc();
		}

		/// <summary>
		/// Resets the enumerator to its initial position (before the first element).
		/// </summary>
		public void Reset() => index = -1;

		/// <summary>
		/// No-op dispose; exists only to satisfy <see cref="IDisposable"/>.
		/// </summary>
		public void Dispose() { }
	}
}