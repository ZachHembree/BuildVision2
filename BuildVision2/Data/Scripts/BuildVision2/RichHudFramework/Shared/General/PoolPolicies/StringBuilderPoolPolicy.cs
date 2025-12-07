using System;
using System.Collections.Generic;
using System.Text;
using VRage;

namespace RichHudFramework
{
	/// <summary>
	/// <see cref="IPooledObjectPolicy{T}"/> implementation for <see cref="StringBuilder"/>.
	/// Reuses instances by clearing them instead of allocating new ones.
	/// </summary>
	public class StringBuilderPoolPolicy : IPooledObjectPolicy<StringBuilder>
	{
		public StringBuilder GetNewObject() => new StringBuilder();

		public void ResetObject(StringBuilder sb) => sb.Clear();

		public void ResetRange(IReadOnlyList<StringBuilder> objects, int index, int count)
		{
			int end = Math.Min(index + count, objects.Count);
			for (int i = index; i < end; i++)
				objects[i].Clear();
		}

		public void ResetRange<T2>(IReadOnlyList<MyTuple<StringBuilder, T2>> objects, int index, int count)
		{
			int end = Math.Min(index + count, objects.Count);
			for (int i = index; i < end; i++)
				objects[i].Item1.Clear();
		}

		/// <summary>
		/// Convenience factory returning a pool pre-configured with this policy.
		/// </summary>
		public static ObjectPool<StringBuilder> GetNewPool() => new ObjectPool<StringBuilder>(new StringBuilderPoolPolicy());
	}
}