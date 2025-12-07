using System;

namespace RichHudFramework
{
	public static partial class Utils
	{
		/// <summary>
		/// Debugging and runtime assertion utilities.
		/// </summary>
		public static class Debug
		{
			/// <summary>
			/// Throws an exception if the given reference is null.
			/// </summary>
			/// <typeparam name="T">Type of the object being checked (must be a reference type or nullable).</typeparam>
			/// <param name="obj">The object instance to verify.</param>
			/// <param name="message">Optional additional information to include in the exception message.</param>
			/// <exception cref="Exception">Thrown when <paramref name="obj"/> is null.</exception>
			public static void AssertNotNull<T>(T obj, string message = "")
			{
				Assert(obj != null, $"Object of type {typeof(T)} is null. " + message);
			}

			/// <summary>
			/// Throws an exception if the specified condition evaluates to false.
			/// </summary>
			/// <param name="condition">The boolean condition that must be true for the call to succeed.</param>
			/// <param name="message">Optional descriptive message included in the exception.</param>
			/// <exception cref="Exception">Thrown when <paramref name="condition"/> is false.</exception>
			public static void Assert(bool condition, string message = "")
			{
				if (!condition)
					throw new Exception("Assertion failed. " + message);
			}
		}
	}
}