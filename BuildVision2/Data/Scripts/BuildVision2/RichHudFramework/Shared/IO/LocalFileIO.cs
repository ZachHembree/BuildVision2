using Sandbox.ModAPI;
using System;
using System.IO;

namespace RichHudFramework.IO
{
	/// <summary>
	/// Handles basic file I/O operations in the mod's local storage. 
	/// <para>Ensures thread safety by preventing concurrent operations on this specific instance.</para>
	/// <para>Wrapper around <see cref="MyAPIGateway.Utilities"/> local storage file utilities.</para>
	/// </summary>
	public class LocalFileIO
	{
		/// <summary>
		/// Returns true if the file exists in the local storage directory.
		/// </summary>
		public bool FileExists => MyAPIGateway.Utilities.FileExistsInLocalStorage(file, typeof(LocalFileIO));

		/// <summary>
		/// The immutable relative file path.
		/// </summary>
		public readonly string file;

		private readonly object fileLock;

		/// <summary>
		/// Initializes a new file handler for the specified path in local storage.
		/// </summary>
		/// <param name="file">Relative path to the file.</param>
		public LocalFileIO(string file)
		{
			this.file = file;
			fileLock = new object();
		}

		/// <summary>
		/// Creates a copy of the current file with a new name in local storage.
		/// </summary>
		/// <param name="newName">The name/path for the duplicate file.</param>
		/// <returns>Returns a <see cref="KnownException"/> if the read or write operation fails.</returns>
		public KnownException TryDuplicate(string newName)
		{
			string data;
			KnownException exception = TryRead(out data);
			LocalFileIO newFile;

			if (exception == null && data != null)
			{
				newFile = new LocalFileIO(newName);
				exception = newFile.TryWrite(data);
			}

			return exception;
		}

		/// <summary>
		/// Attempts to append a string to the existing local file.
		/// <para><strong>Warning:</strong> This performs a full read-modify-write cycle 
		/// (reads existing, concatenates, writes all).</para>
		/// </summary>
		/// <param name="data">The string to append.</param>
		/// <returns>Returns a <see cref="KnownException"/> if the operation fails.</returns>
		public KnownException TryAppend(string data)
		{
			string current;
			KnownException exception = TryRead(out current);

			if (exception == null && current != null)
			{
				current += data;
				exception = TryWrite(current);
			}
			else
				exception = TryWrite(data);

			return exception;
		}

		/// <summary>
		/// Attempts to retrieve the file data as a byte array. 
		/// <para><strong>Note:</strong> Expects the file to start with a 32-bit integer indicating the array length.</para>
		/// </summary>
		/// <param name="stream">The byte array read from the file.</param>
		/// <returns>Returns a <see cref="KnownException"/> if an error occurs.</returns>
		public KnownException TryRead(out byte[] stream)
		{
			KnownException exception = null;
			BinaryReader reader = null;

			lock (fileLock)
			{
				try
				{
					reader = MyAPIGateway.Utilities.ReadBinaryFileInLocalStorage(file, typeof(LocalFileIO));
					// Reads length prefix first, then the data
					stream = reader.ReadBytes(reader.ReadInt32());
				}
				catch (Exception e)
				{
					stream = null;
					exception = new KnownException($"IO Error. Unable to read from {file}.", e);
				}
				finally
				{
					reader?.Close();
				}
			}

			return exception;
		}

		/// <summary>
		/// Attempts to retrieve the file data as a string.
		/// </summary>
		/// <param name="data">The string content of the file.</param>
		/// <returns>Returns a <see cref="KnownException"/> if an error occurs.</returns>
		public KnownException TryRead(out string data)
		{
			KnownException exception = null;
			TextReader reader = null;
			data = null;

			lock (fileLock)
			{
				try
				{
					reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(file, typeof(LocalFileIO));
					data = reader.ReadToEnd();
				}
				catch (Exception e)
				{
					data = null;
					exception = new KnownException($"IO Error. Unable to read from {file}.", e);
				}
				finally
				{
					reader?.Close();
				}
			}

			return exception;
		}

		/// <summary>
		/// Attempts to write a byte array to the file. 
		/// <para><strong>Note:</strong> Prepends the size of the array (Int32) to the file header before writing data.</para>
		/// </summary>
		/// <param name="stream">The byte array to write.</param>
		/// <returns>Returns a <see cref="KnownException"/> if the write fails.</returns>
		public KnownException TryWrite(byte[] stream)
		{
			KnownException exception = null;
			BinaryWriter writer = null;

			lock (fileLock)
			{
				try
				{
					writer = MyAPIGateway.Utilities.WriteBinaryFileInLocalStorage(file, typeof(LocalFileIO));
					writer.Write(stream.Length); // Write length prefix
					writer.Write(stream);
					writer.Flush();
				}
				catch (Exception e)
				{
					exception = new KnownException($"IO Error. Unable to write to {file}.", e);
				}
				finally
				{
					writer?.Close();
				}
			}

			return exception;
		}

		/// <summary>
		/// Attempts to overwrite the file with the provided string data.
		/// </summary>
		/// <param name="data">The text data to write.</param>
		/// <returns>Returns a <see cref="KnownException"/> if the write fails.</returns>
		public KnownException TryWrite(string data)
		{
			KnownException exception = null;
			TextWriter writer = null;

			lock (fileLock)
			{
				try
				{
					writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(file, typeof(LocalFileIO));
					writer.Write(data);
					writer.Flush();
				}
				catch (Exception e)
				{
					exception = new KnownException($"IO Error. Unable to write to {file}.", e);
				}
				finally
				{
					writer?.Close();
				}
			}

			return exception;
		}
	}
}