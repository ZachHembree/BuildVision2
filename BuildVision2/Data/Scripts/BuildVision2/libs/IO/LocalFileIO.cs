using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sandbox.ModAPI;
using System.Xml.Serialization;

namespace DarkHelmet.IO
{
    /// <summary>
    /// Handles basic file I/O operations in local storage. Will not allow multiple threads to operate on the same file object concurrently.
    /// </summary>
    internal class LocalFileIO
    {
        public readonly string file;
        private readonly object fileLock;

        public LocalFileIO(string file)
        {
            this.file = file;
            fileLock = new object();
        }

        /// <summary>
        /// Creates a local duplicate of a file with a given name.
        /// </summary>
        public DhException TryDuplicate(string newName)
        {
            string data;
            DhException exception = TryRead(out data);
            LocalFileIO newFile;

            if (exception == null && data != null)
            {
                newFile = new LocalFileIO(newName);
                exception = newFile.TryWrite(data);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to append string to an existing local file.
        /// </summary>
        public DhException TryAppend(string data)
        {
            string current;
            DhException exception = TryRead(out current);

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
        /// Attempts to retrieve local file data.
        /// </summary>
        public DhException TryRead(out string data)
        {
            DhException exception = null;
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
                    exception = new DhException($"IO Error. Unable to read from {file}.", e);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }

            return exception;
        }

        /// <summary>
        /// Attempts to write data to a local file.
        /// </summary>
        public DhException TryWrite(string data)
        {
            DhException exception = null;
            TextWriter writer = null;
            
            lock (fileLock)
            {
                try
                {
                    writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(file, typeof(LocalFileIO));
                    writer.WriteLine(data);
                    writer.Flush();
                }
                catch (Exception e)
                {
                    exception = new DhException($"IO Error. Unable to write to {file}.", e);
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }
            }

            return exception;
        }
    }
}
