using System;
using System.Collections.Generic;

namespace DarkHelmet.IO
{
    /// <summary>
    /// Handles logging; singleton
    /// </summary>
    internal sealed class LogIO
    {
        public static LogIO Instance { get; private set; }
        public bool Accessible { get; private set; }

        private readonly Action<string> SendMessage;
        private readonly LocalFileIO logFile;
        private readonly TaskPool taskPool;

        private LogIO(string fileName, Action<string> SendMessage)
        {
            Accessible = true;
            logFile = new LocalFileIO(fileName);
            this.SendMessage = SendMessage;

            taskPool = new TaskPool(1, ErrorCallback);
        }

        public static void Init(string fileName, Action<string> SendMessage)
        {
            if (Instance == null)
                Instance = new LogIO(fileName, SendMessage);
        }

        public void Close()
        {
            Instance = null;
        }

        /// <summary>
        /// Updates internal task queue. Parallel methods will not work properly if this isn't being
        /// updated regularly.
        /// </summary>
        public void Update() =>
            taskPool.Update();

        private void ErrorCallback(List<IOException> known, AggregateException unknown)
        {
            if ((known != null && known.Count > 0) || unknown != null)
            {
                TryWriteToLogFinish(false);

                if (known != null && known.Count > 0)
                    foreach (Exception e in known)
                        SendMessage(e.Message);

                if (unknown != null)
                    throw unknown;
            }
        }

        /// <summary>
        /// Attempts to synchronously update log with message and adds a time stamp.
        /// </summary>
        public bool TryWriteToLog(string message)
        {
            if (Accessible)
            {
                message = $"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ms")}] {message}";
                IOException exception = logFile.TryAppend(message);

                if (exception != null)
                {
                    SendMessage("Unable to update log; please check your file access permissions.");
                    Accessible = false;
                    throw exception;
                }
                else
                {
                    SendMessage("Log updated.");
                    Accessible = true;
                    return true;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Attempts to update log in parallel with message and adds a time stamp.
        /// </summary>
        public void TryWriteToLogStart(string message)
        {
            if (Accessible)
            {
                message = $"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ms")}] {message}";

                taskPool.EnqueueTask(() =>
                {
                    IOException exception = logFile.TryAppend(message);

                    if (exception != null)
                    {
                        taskPool.EnqueueAction(() => TryWriteToLogFinish(false));
                        throw exception;
                    }
                    else
                        taskPool.EnqueueAction(() => TryWriteToLogFinish(true));
                });
            }
        }

        private void TryWriteToLogFinish(bool success)
        {
            if (!success)
            {
                if (Accessible)
                    SendMessage("Unable to update log; please check your file access permissions.");

                Accessible = false;
            }
            else
            {
                if (Accessible)
                    SendMessage("Log updated.");

                Accessible = true;
            }
        }               
    }
}