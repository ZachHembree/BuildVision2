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

        private readonly BvMain main;
        private readonly LocalFileIO logFile;
        private readonly TaskPool taskPool;

        private LogIO(BvMain main, string fileName)
        {
            Accessible = true;
            this.main = main;
            logFile = new LocalFileIO(fileName);
            taskPool = new TaskPool(1, ErrorCallback);
        }

        public static LogIO GetInstance(BvMain main, string fileName)
        {
            if (Instance == null)
                Instance = new LogIO(main, fileName);

            return Instance;
        }

        /// <summary>
        /// Updates internal thread pool. Parallel methods will not work properly if this isn't being
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
                        main.SendChatMessage(e.Message);

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
                    main.SendChatMessage("Unable to update log; please check your file access permissions.");

                Accessible = false;
            }
            else
            {
                if (Accessible)
                    main.SendChatMessage("Log updated.");

                Accessible = true;
            }
        }               
    }
}