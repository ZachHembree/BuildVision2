using Sandbox.ModAPI;
using System;

namespace RichHudFramework
{
	public static partial class Utils
	{
		/// <summary>
		/// Safe wrappers around Space Engineers' built-in ProtoBuf serialization utilities.
		/// Catches exceptions and returns them as <see cref="KnownException"/> instead of letting them propagate.
		/// </summary>
		public static class ProtoBuf
		{
			/// <summary>
			/// Attempts to serialize an object to a binary byte array using ProtoBuf.
			/// <para>Wraps MyAPIGateway.Utilities.SerializeToBinary().</para>
			/// </summary>
			/// <typeparam name="T">
			/// Type of the object to serialize. Must be marked [ProtoContract] with members 
			/// marked [ProtoMember(uniqueID)].
			/// </typeparam>
			/// <param name="obj">The object instance to serialize.</param>
			/// <param name="dataOut">Receives the serialized byte array on success; null on failure.</param>
			/// <returns>Null on success, otherwise a <see cref="KnownException"/> describing the failure.</returns>
			public static KnownException TrySerialize<T>(T obj, out byte[] dataOut)
			{
				KnownException exception = null;
				dataOut = null;
				
				try
				{
					dataOut = MyAPIGateway.Utilities.SerializeToBinary(obj);
				}
				catch (Exception e)
				{
					exception = new KnownException($"IO Error. Failed to generate binary from {typeof(T).Name}.", e);
				}

				return exception;
			}

			/// <summary>
			/// Attempts to deserialize a byte array into an object of the specified type using ProtoBuf.
			/// <para>Wraps MyAPIGateway.Utilities.SerializeFromBinary()</para>
			/// </summary>
			/// <typeparam name="T">Target type. Must match the type used during serialization.</typeparam>
			/// <param name="dataIn">The byte array containing serialized data.</param>
			/// <param name="obj">Receives the deserialized instance on success; default(T) on failure.</param>
			/// <returns>Null on success, otherwise a <see cref="KnownException"/> describing the failure.</returns>
			public static KnownException TryDeserialize<T>(byte[] dataIn, out T obj)
			{
				KnownException exception = null;
				obj = default(T);

				try
				{
					obj = MyAPIGateway.Utilities.SerializeFromBinary<T>(dataIn);
				}
				catch (Exception e)
				{
					exception = new KnownException($"IO Error. Failed to deserialize to {typeof(T).Name}.", e);
				}

				return exception;
			}
		}
	}
}