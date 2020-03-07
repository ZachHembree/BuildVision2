using RichHudFramework.Game;
using System;
using System.Collections.Generic;

namespace RichHudFramework.IO
{
    /// <summary>
    /// Handles logging
    /// </summary>
    public sealed class LogIO : InternalParallelComponentBase
    {
        public static bool Accessible => Instance.accessible;
        public static string FileName 
        { 
            get { return _fileName; } 
            set 
            {
                if (value != _fileName)
                    Instance.logFile = new LocalFileIO(value);

                _fileName = value;
            }
        }

        private static LogIO Instance
        { 
            get 
            { 
                if (_instance == null) 
                    Init(); 

                return _instance; 
            } 
            set { _instance = value; }
        }

        private static LogIO _instance;
        private static string _fileName;

        public bool accessible;
        private LocalFileIO logFile;

        private LogIO() : base(true, true)
        {
            accessible = true;
            logFile = new LocalFileIO(_fileName);
        }

        private static void Init()
        {
            if (_instance == null)
            {
                _instance = new LogIO();
            }
        }

        protected override void ErrorCallback(List<KnownException> known, AggregateException unknown)
        {
            if ((known != null && known.Count > 0) || unknown != null)
            {
                WriteToLogFinish(false);

                if (known != null && known.Count > 0)
                    foreach (Exception e in known)
                        SendChatMessage(e.Message);

                if (unknown != null)
                    throw unknown;
            }
        }

        public new static bool TryWriteToLog(string message) =>
            Instance.TryWriteToLogInternal(message);

        public new static void WriteToLogStart(string message) =>
            Instance.WriteToLogStartInternal(message);

        /// <summary>
        /// Attempts to synchronously update log with message and adds a time stamp.
        /// </summary>
        public bool TryWriteToLogInternal(string message)
        {
            if (accessible)
            {
                message = $"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ms")}] {message}";
                KnownException exception = logFile.TryAppend(message);

                if (exception != null)
                {
                    SendChatMessage("Unable to update log; please check your file access permissions.");
                    accessible = false;
                    throw exception;
                }
                else
                {
                    SendChatMessage("Log updated.");
                    accessible = true;
                    return true;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Attempts to update log in parallel with message and adds a time stamp.
        /// </summary>
        public void WriteToLogStartInternal(string message)
        {
            if (accessible)
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
                if (accessible)
                    SendChatMessage("Unable to update log; please check your file access permissions.");

                accessible = false;
            }
            else
            {
                if (accessible)
                    SendChatMessage("Log updated.");

                accessible = true;
            }
        }
    }
}