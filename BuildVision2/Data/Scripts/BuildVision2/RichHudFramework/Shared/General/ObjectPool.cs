using System;
using System.Collections.Generic;
using VRage;

namespace RichHudFramework
{
	/// <summary>
	/// Defines the creation and reset behavior for objects managed by an <see cref="ObjectPool{T}"/>.
	/// </summary>
	public interface IPooledObjectPolicy<T>
	{
		/// <summary>
		/// Creates a fresh instance of <typeparamref name="T"/>.
		/// </summary>
		T GetNewObject();

		/// <summary>
		/// Prepares a used object for reuse (e.g. clears state, unsubscribes events).
		/// Called immediately before the object is returned to the pool.
		/// </summary>
		void ResetObject(T obj);

		/// <summary>
		/// Resets a contiguous range of pooled objects in a list.
		/// </summary>
		void ResetRange(IReadOnlyList<T> objects, int index, int count);

		/// <summary>
		/// Resets the contiguous range of pooled objects in a list of tuples.
		/// </summary>
		void ResetRange<T2>(IReadOnlyList<MyTuple<T, T2>> objects, int index, int count);
	}

	/// <summary>
	/// Delegate-based implementation of <see cref="IPooledObjectPolicy{T}"/>.
	/// </summary>
	public class PooledObjectPolicy<T> : IPooledObjectPolicy<T>
	{
		private readonly Func<T> getNewObjectFunc;
		private readonly Action<T> resetObjectAction;

		public PooledObjectPolicy(Func<T> getNewObjectFunc, Action<T> resetObjectAction)
		{
			if (getNewObjectFunc == null || resetObjectAction == null)
				throw new ArgumentNullException();

			this.getNewObjectFunc = getNewObjectFunc;
			this.resetObjectAction = resetObjectAction;
		}

		public T GetNewObject() => getNewObjectFunc();
		public void ResetObject(T obj) => resetObjectAction(obj);

		public void ResetRange(IReadOnlyList<T> objects, int index, int count)
		{
			int end = Math.Min(index + count, objects.Count);
			for (int i = index; i < end; i++)
				resetObjectAction(objects[i]);
		}

		public void ResetRange<T2>(IReadOnlyList<MyTuple<T, T2>> objects, int index, int count)
		{
			int end = Math.Min(index + count, objects.Count);
			for (int i = index; i < end; i++)
				resetObjectAction(objects[i].Item1);
		}
	}

	/// <summary>
	/// Simple, non-thread-safe object pool backed by a <see cref="List{T}"/>.
	/// Reduces allocations for frequently created/disposed reference types.
	/// </summary>
	public class ObjectPool<T> where T : class
	{
		/// <summary>
		/// Number of objects currently stored in the pool
		/// </summary>
		public int Count => pooledObjects.Count;

		/// <summary>
		/// Internal pool capacity
		/// </summary>
		public int Capacity => pooledObjects.Capacity;

		/// <exclude/>
		protected readonly List<T> pooledObjects;
		/// <exclude/>
		protected readonly IPooledObjectPolicy<T> policy;

		public ObjectPool(IPooledObjectPolicy<T> policy)
		{
			if (policy == null)
				throw new Exception("Pooled object policy cannot be null.");

			pooledObjects = new List<T>();
			this.policy = policy;
		}

		public ObjectPool(Func<T> getNewFunc, Action<T> resetFunc)
			: this(new PooledObjectPolicy<T>(getNewFunc, resetFunc))
		{ }

		/// <summary>
		/// Retrieves an object from the pool. Returns a recycled instance if available;
		/// otherwise creates a new one using the policy.
		/// </summary>
		public T Get()
		{
			if (pooledObjects.Count > 0)
			{
				int last = pooledObjects.Count - 1;
				T obj = pooledObjects[last];
				pooledObjects.RemoveAt(last);
				return obj;
			}

			return policy.GetNewObject();
		}

		/// <summary>
		/// Returns a single object to the pool after resetting it.
		/// </summary>
		public void Return(T obj)
		{
			policy.ResetObject(obj);
			pooledObjects.Add(obj);
		}

		/// <summary>
		/// Returns a range of objects to the pool
		/// </summary>
		public void ReturnRange(IReadOnlyList<T> objects, int index = 0, int count = -1)
		{
			if (count == -1) count = objects.Count - index;
			if (count <= 0) return;

			policy.ResetRange(objects, index, count);

			int end = index + count - 1;
			for (int i = 0; i < count; i++)
				pooledObjects.Add(objects[end - i]);
		}

		/// <summary>
		/// Returns a range of objects to the pool from a tuple list
		/// </summary>
		public void ReturnRange<T2>(IReadOnlyList<MyTuple<T, T2>> objects, int index = 0, int count = -1)
		{
			if (count == -1) count = objects.Count - index;
			if (count <= 0) return;

			policy.ResetRange(objects, index, count);

			int end = index + count - 1;
			for (int i = 0; i < count; i++)
				pooledObjects.Add(objects[end - i].Item1);
		}

		public void TrimExcess() => pooledObjects.TrimExcess();
		public void Clear() => pooledObjects.Clear();
	}
}