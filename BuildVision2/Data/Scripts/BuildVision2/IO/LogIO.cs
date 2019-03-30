using System;
using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Handles logging; singleton
    /// </summary>
    internal sealed class LogIO
    {
        public bool Accessible { get; private set; }
        public static LogIO Instance { get; private set; }

        private static BvMain Main { get { return BvMain.Instance; } }
        private readonly LocalFileIO logFile;
        private readonly TaskPool taskPool;

        private LogIO(string fileName)
        {
            Accessible = true;
            logFile = new LocalFileIO(fileName);
            taskPool = new TaskPool(1, ErrorCallback);
        }

        public static LogIO GetInstance(string fileName)
        {
            if (Instance == null)
                Instance = new LogIO(fileName);

            return Instance;
        }

        /// <summary>
        /// Updates internal task queue. Parallel methods will not work properly if this isn't being
        /// updated regularly.
        /// </summary>
        public void Update() =>
            taskPool.Update();

        private void ErrorCallback(List<BvException> known, BvAggregateException unknown)
        {
            if ((known != null && known.Count > 0) || unknown != null)
            {
                TryWriteToLogFinish(false);

                if (known != null && known.Count > 0)
                    foreach (Exception e in known)
                        Main.SendChatMessage(e.Message);

                if (unknown != null)
                    throw unknown;
            }
        }

        public void Close()
        {
            Instance = null;
        }

        /// <summary>
        /// Attempts to synchronously update log with message and adds a time stamp.
        /// </summary>
        public bool TryWriteToLog(string message)
        {
            if (Accessible)
            {
                message = $"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ms")}] {message}";

                if (logFile.TryAppend(message) != null)
                {
                    Accessible = false;
                    return false;
                }
                else
                {
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
                    BvException exception = logFile.TryAppend(message);

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
                    Main.SendChatMessage("Unable to update log; please check your file access permissions.");

                Accessible = false;
            }
            else
            {
                if (Accessible)
                    Main.SendChatMessage("Log updated.");

                Accessible = true;
            }
        }               
    }
}