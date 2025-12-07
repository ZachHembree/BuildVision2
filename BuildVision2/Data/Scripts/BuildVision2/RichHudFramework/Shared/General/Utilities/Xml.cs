using Sandbox.ModAPI;
using System;

namespace RichHudFramework
{
	public static partial class Utils
	{
		/// <summary>
		/// Safe wrappers around Space Engineers' built-in XML serialization utilities.
		/// Exceptions are caught and returned as <see cref="KnownException"/> rather than thrown.
		/// </summary>
		public static class Xml
		{
			/// <summary>
			/// Attempts to serialize an object to an XML string.
			/// <para>Wraps MyAPIGateway.Utilities.SerializeToXML()</para>
			/// </summary>
			/// <typeparam name="T">Type of object to serialize (usually a plain data class).</typeparam>
			/// <param name="obj">The instance to serialize.</param>
			/// <param name="xmlOut">Receives the XML string on success; null on failure.</param>
			/// <returns>Null on success; otherwise a <see cref="KnownException"/> describing the error.</returns>
			public static KnownException TrySerialize<T>(T obj, out string xmlOut)
			{
				KnownException exception = null;
				xmlOut = null;

				try
				{
					xmlOut = MyAPIGateway.Utilities.SerializeToXML(obj);
				}
				catch (Exception e)
				{
					exception = new KnownException("IO Error. Failed to generate XML.", e);
				}

				return exception;
			}

			/// <summary>
			/// Attempts to deserialize an XML string into an object of the specified type.
			/// <para>Wraps MyAPIGateway.Utilities.SerializeFromXML()</para>
			/// </summary>
			/// <typeparam name="T">Target type matching the original serialized object.</typeparam>
			/// <param name="xmlIn">The XML string to deserialize.</param>
			/// <param name="obj">Receives the deserialized instance on success; default(T) on failure.</param>
			/// <returns>Null on success; otherwise a <see cref="KnownException"/> describing the error.</returns>
			public static KnownException TryDeserialize<T>(string xmlIn, out T obj)
			{
				KnownException exception = null;
				obj = default(T);

				try
				{
					obj = MyAPIGateway.Utilities.SerializeFromXML<T>(xmlIn);
				}
				catch (Exception e)
				{
					exception = new KnownException("IO Error. Unable to interpret XML.", e);
				}

				return exception;
			}
		}
	}
}