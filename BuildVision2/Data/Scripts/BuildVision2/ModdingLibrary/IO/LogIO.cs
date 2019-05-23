using System;
using System.Collections.Generic;
using DarkHelmet.Game;

namespace DarkHelmet.IO
{
    /// <summary>
    /// Handles logging; singleton
    /// </summary>
    public sealed class LogIO : ModBase.Component<LogIO>
    {
        public static string FileName { get { return fileName; } set { if (value != null && value.Length > 0) fileName = value; } }
        private static string fileName = "modLog.txt";

        public bool Accessible { get; private set; }

        private readonly LocalFileIO logFile;
        private readonly TaskPool taskPool;

        static LogIO()
        {
            UpdateActions.Add(() => Instance.taskPool.Update());
        }

        public LogIO()
        {
            Accessible = true;
            logFile = new LocalFileIO(FileName);
            taskPool = new TaskPool(1, ErrorCallback);
        }

        private void ErrorCallback(List<IOException> known, AggregateException unknown)
        {
            if ((known != null && known.Count > 0) || unknown != null)
            {
                TryWriteToLogFinish(false);

                if (known != null && known.Count > 0)
                    foreach (Exception e in known)
                        ModBase.SendChatMessage(e.Message);

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
                    ModBase.SendChatMessage("Unable to update log; please check your file access permissions.");
                    Accessible = false;
                    throw exception;
                }
                else
                {
                    ModBase.SendChatMessage("Log updated.");
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
                    ModBase.SendChatMessage("Unable to update log; please check your file access permissions.");

                Accessible = false;
            }
            else
            {
                if (Accessible)
                    ModBase.SendChatMessage("Log updated.");

                Accessible = true;
            }
        }               
    }
}