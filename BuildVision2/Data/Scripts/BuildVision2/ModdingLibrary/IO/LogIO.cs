using System;
using System.Collections.Generic;
using DarkHelmet.Game;

namespace DarkHelmet.IO
{
    /// <summary>
    /// Handles logging
    /// </summary>
    public sealed class LogIO : ModBase.ParallelComponent<LogIO>
    {
        public static string FileName { get; set; } 
        public bool Accessible { get; private set; }
        private readonly LocalFileIO logFile;

        public LogIO()
        {
            Accessible = true;
            logFile = new LocalFileIO(FileName);
        }

        protected override void ErrorCallback(List<KnownException> known, AggregateException unknown)
        {
            if ((known != null && known.Count > 0) || unknown != null)
            {
                WriteToLogFinish(false);

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
                KnownException exception = logFile.TryAppend(message);

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
        public void WriteToLogStart(string message)
        {
            if (Accessible)
            {
                message = $"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ms")}] {message}";

                EnqueueTask(() =>
                {
                    KnownException exception = logFile.TryAppend(message);

                    if (exception != null)
                    {
                        EnqueueAction(() => WriteToLogFinish(false));
                        throw exception;
                    }
                    else
                        EnqueueAction(() => WriteToLogFinish(true));
                });
            }
        }

        private void WriteToLogFinish(bool success)
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