using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sandbox.ModAPI;
using System.Xml.Serialization;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// This class should not exist. I should have access to the .NET framework
    /// AggregateException class, but here we are.
    /// </summary>
    internal class BvAggregateException : Exception
    {
        public BvAggregateException(string aggregatedMsg) : base(aggregatedMsg)
        { }

        public BvAggregateException(IList<Exception> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        public BvAggregateException(IList<BvException> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        public BvAggregateException(IList<BvAggregateException> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        private static string GetExceptionMessages(IList<BvException> exceptions)
        {
            StringBuilder sb = new StringBuilder();

            for (int n = 0; n < exceptions.Count; n++)
                if (n != exceptions.Count - 1)
                    sb.Append($"{exceptions[n].ToString()}\n");
                else
                    sb.Append($"{exceptions[n].ToString()}");

            return sb.ToString();
        }

        private static string GetExceptionMessages(IList<Exception> exceptions)
        {
            StringBuilder sb = new StringBuilder();

            for (int n = 0; n < exceptions.Count; n++)
                if (n != exceptions.Count - 1)
                    sb.Append($"{exceptions[n].ToString()}\n");
                else
                    sb.Append($"{exceptions[n].ToString()}");

            return sb.ToString();
        }

        private static string GetExceptionMessages(IList<BvAggregateException> exceptions)
        {
            StringBuilder sb = new StringBuilder();

            for (int n = 0; n < exceptions.Count; n++)
                if (n != exceptions.Count - 1)
                    sb.Append($"{exceptions[n].ToString()}\n");
                else
                    sb.Append($"{exceptions[n].ToString()}");

            return sb.ToString();
        }
    }

    internal class BvException : Exception
    {
        public BvException() : base()
        { }

        public BvException(string message) : base(message)
        { }

        public BvException(string message, Exception innerException) : base(message, innerException)
        { }
    }

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
        public BvException TryDuplicate(string newName)
        {
            string data;
            BvException exception = TryRead(out data);
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
        public BvException TryAppend(string data)
        {
            string current;
            BvException exception = TryRead(out current);

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
        public BvException TryRead(out string data)
        {
            BvException exception = null;
            TextReader reader = null;
            data = null;

            lock (fileLock)
            {
                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(file, typeof(LocalFileIO)))
                {
                    try
                    {
                        reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(file, typeof(LocalFileIO));
                        data = reader.ReadToEnd();
                    }
                    catch (Exception e)
                    {
                        data = null;
                        exception = new BvException($"IO Error. Unable to read from {file}.", e);
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
            }

            return exception;
        }

        /// <summary>
        /// Attempts to write data to a local file.
        /// </summary>
        public BvException TryWrite(string data)
        {
            BvException exception = null;
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
                    exception = new BvException($"IO Error. Unable to write to {file}.", e);
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
