using Sandbox.ModAPI;
using System;
using System.IO;
using VRage.Game;

namespace RichHudFramework.IO
{
	/// <summary>
	/// Handles read-only file operations for files located within the mod's directory.
	/// <para>Wrapper around <see cref="MyAPIGateway.Utilities"/> mod location file utils.</para>
	/// </summary>
	public class ReadOnlyModFileIO
	{
		/// <summary>
		/// Returns true if the file exists in the specified mod location.
		/// </summary>
		public bool FileExists => MyAPIGateway.Utilities.FileExistsInModLocation(file, mod);

		/// <summary>
		/// The relative file path inside the mod folder.
		/// </summary>
		public readonly string file;

		/// <summary>
		/// The specific mod context (workshop item or local mod) containing the file.
		/// </summary>
		public readonly MyObjectBuilder_Checkpoint.ModItem mod;

		/// <summary>
		/// Initializes a new instance of the file reader for a specific mod context.
		/// </summary>
		/// <param name="file">The relative path to the file within the mod.</param>
		/// <param name="mod">The mod item context.</param>
		public ReadOnlyModFileIO(string file, MyObjectBuilder_Checkpoint.ModItem mod)
		{
			this.file = file;
			this.mod = mod;
		}

		/// <summary>
		/// Attempts to retrieve the file data as a byte array. 
		/// <para><strong>Note:</strong> This method expects the file to begin with a signed 32-bit integer 
		/// indicating the length of the byte array.</para>
		/// </summary>
		/// <param name="stream">The byte array read from the file, or null if the operation fails.</param>
		/// <returns>Returns a <see cref="KnownException"/> if an error occurs, otherwise returns null.</returns>
		public KnownException TryRead(out byte[] stream)
		{
			KnownException exception = null;
			BinaryReader reader = null;

			try
			{
				reader = MyAPIGateway.Utilities.ReadBinaryFileInModLocation(file, mod);
				// Expects Int32 length prefix before the data
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

			return exception;
		}

		/// <summary>
		/// Attempts to retrieve the file data as a string.
		/// </summary>
		/// <param name="data">The string content read from the file, or null if the operation fails.</param>
		/// <returns>Returns a <see cref="KnownException"/> if an error occurs, otherwise returns null.</returns>
		public KnownException TryRead(out string data)
		{
			KnownException exception = null;
			TextReader reader = null;
			data = null;

			try
			{
				reader = MyAPIGateway.Utilities.ReadFileInModLocation(file, mod);
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

			return exception;
		}
	}
}