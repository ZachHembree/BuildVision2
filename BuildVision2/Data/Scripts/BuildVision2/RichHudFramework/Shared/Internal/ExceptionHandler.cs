using RichHudFramework.IO;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game;

namespace RichHudFramework.Internal
{
    /// <summary>
    /// Handles exceptions for session components extending from ModBase.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public sealed class ExceptionHandler : MySessionComponentBase
    {
        /// <summary>
        /// Sets the mod name to be used in chat messages, popups and anything else that might require it.
        /// </summary>
        public static string ModName { get; set; }

        /// <summary>
        /// The maximum number of times the mod will be allowed to reload as a result of an unhandled exception.
        /// </summary>
        public static int RecoveryLimit { get; set; }

        /// <summary>
        /// The number of times the handler has reloaded its clients in response to unhandled exceptions.
        /// </summary>
        public static int RecoveryAttempts { get; private set; }

        /// <summary>
        /// If set to true, the user will be given the option to reload in the event of an
        /// unhandled exception.
        /// </summary>
        public static bool PromptForReload { get; set; }

        /// <summary>
        /// True if the handler is currently in the process of reloading its clients.
        /// </summary>
        public static bool Reloading { get; private set; }

        /// <summary>
        /// True if the handler is currently in the process of unloading its clients.
        /// </summary>
        public static bool Unloading { get; private set; }

        public static bool ClientsPaused { get; private set; }

        private static ExceptionHandler instance;
        private const long exceptionReportInterval = 100, exceptionLoopTime = 50;
        private const int exceptionLoopCount = 10;

        private int exceptionCount;
        private readonly List<ModBase> clients;
        private readonly List<string> exceptionMessages;
        private readonly Utils.Stopwatch errorTimer;

        private Action lastMissionScreen;

        public ExceptionHandler()
        {
            if (instance == null)
                instance = this;
            else
                throw new Exception("Only one instance of ExceptionHandler can exist at any given time.");

            ModName = DebugName;
            RecoveryLimit = 1;

            exceptionMessages = new List<string>();
            errorTimer = new Utils.Stopwatch();
            clients = new List<ModBase>();
        }

        /// <summary>
        /// Registers the <see cref="ModBase"/> with the handler if it isn't already registered.
        /// </summary>
        public static void RegisterClient(ModBase client)
        {
            if (!instance.clients.Contains(client))
                instance.clients.Add(client);
        }

        /// <summary>
        /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it, display an error message to the user and reload or unload the mod depending on the configuration.
        /// </summary>
        public static void Run(Action Action)
        {
            try
            {
                Action();
            }
            catch (Exception e)
            {
                instance.ReportException(e);
            }
        }

        /// <summary>
        /// Executes a given <see cref="Func{TResult}"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it, display an error message to the user and reload or unload the mod depending on the configuration.
        /// </summary>
        public static TResult Run<TResult>(Func<TResult> Func)
        {
            TResult value = default(TResult);

            try
            {
                value = Func();
            }
            catch (Exception e)
            {
                instance.ReportException(e);
            }

            return value;
        }

        /// <summary>
        /// Records exceptions to be handled. Duplicate stack traces are excluded from the log entry.
        /// </summary>
        private void ReportException(Exception e)
        {
            string message = e.ToString();

            if (!exceptionMessages.Contains(message))
                exceptionMessages.Add(message);

            if (exceptionCount == 0)
                errorTimer.Start();

            exceptionCount++;

            // Exception loop, respond immediately
            if (exceptionCount > exceptionLoopCount && errorTimer.ElapsedMilliseconds < exceptionLoopTime)
                PauseClients();
        }

        public override void Draw()
        {
            if (exceptionCount > 0 && errorTimer.ElapsedMilliseconds > exceptionReportInterval)
                HandleExceptions();

            // This is a workaround. If you try to create a mission screen while the chat is open, 
            // the UI will become unresponsive.
            if (lastMissionScreen != null && !MyAPIGateway.Gui.ChatEntryVisible)
            {
                lastMissionScreen();
                lastMissionScreen = null;
            }
        }

        /// <summary>
        /// Generates an single log entry from the stack traces recorded within the logging interval
        /// and reloads or unloads the clients depending on the handler's current configuration and
        /// the number of recovery attempts.
        /// </summary>
        private void HandleExceptions()
        {
            if (!Unloading && !Reloading)
            {
                string exceptionText = GetExceptionText();
                exceptionCount = 0;

                LogIO.TryWriteToLog(ModName + " encountered an unhandled exception.\n" + exceptionText + '\n');
                exceptionMessages.Clear();

                if (ModBase.IsClient && PromptForReload)
                {
                    if (RecoveryAttempts < RecoveryLimit)
                    {
                        PauseClients();
                        ShowErrorPrompt(exceptionText, true);
                    }
                    else
                    {
                        UnloadClients();
                        ShowErrorPrompt(exceptionText, false);
                    }
                }
                else
                {
                    if (RecoveryAttempts < RecoveryLimit)
                        ReloadClients();
                    else
                        UnloadClients();
                }

                RecoveryAttempts++;
            }
        }

        /// <summary>
        /// Generates final exception text from the list of messages recorded.
        /// </summary>
        private string GetExceptionText()
        {
            StringBuilder errorMessage = new StringBuilder();

            if (exceptionCount > exceptionLoopCount && errorTimer.ElapsedMilliseconds < exceptionLoopTime)
                errorMessage.AppendLine($"[Exception Loop Detected] {exceptionCount} exceptions were reported within a span of {errorTimer.ElapsedMilliseconds}ms.");

            for (int n = 0; n < exceptionMessages.Count - 1; n++)
                errorMessage.AppendLine(exceptionMessages[n]);

            errorMessage.Append(exceptionMessages[exceptionMessages.Count - 1]);

            errorMessage.Replace("--->", "\n   --->");
            return errorMessage.ToString();
        }

        /// <summary>
        /// If canReload == true, the user will be prompted to choose to either reload or cancel reload.
        /// If canReload == false, it will still show the user the error message, but wont give them an option
        /// to reload.
        /// </summary>
        private void ShowErrorPrompt(string errorMessage, bool canReload)
        {
            if (canReload)
            {
                ShowMissionScreen
                (
                    "Debug",
                    $"{ModName} has encountered a problem and will need to reload. Press the X in the upper right hand corner " +
                    "to cancel.\n\n" +
                    "Error Details:\n" +
                    errorMessage,
                    AllowReload,
                    "Reload"
                );
            }
            else
            {
                ShowMissionScreen
                (
                    "Debug",
                    $"{ModName} has encountered an error and was unable to recover.\n\n" +
                    "Error Details:\n" +
                    errorMessage,
                    null,
                    "Close"
                );

                SendChatMessage($"{ModName} has encountered an error and was unable to recover. See log for details.");
            }
        }

        /// <summary>
        /// Error prompt callback. If reload is clicked, it will unpause the clients and reload. Otherwise, the
        /// clients will unload.
        /// </summary>
        private void AllowReload(ResultEnum response)
        {
            if (response == ResultEnum.OK)
                ReloadClients();
            else
                UnloadClients();
        }

        /// <summary>
        /// Creates a message window using the mod name, a given subheading and a message.
        /// </summary>
        public static void ShowMissionScreen(string subHeading = null, string message = null, Action<ResultEnum> callback = null, string okButtonCaption = null)
        {
            Action messageAction = () => MyAPIGateway.Utilities.ShowMissionScreen(ModName, subHeading, null, message, callback, okButtonCaption);
            instance.lastMissionScreen = messageAction;
        }

        /// <summary>
        /// Creates a message window using the mod name, a given subheading and a message.
        /// </summary>
        public static void ShowMessageScreen(string subHeading, string message) =>
            ShowMissionScreen(subHeading, message, null, "Close");

        /// <summary>
        /// Sends chat message using the mod name as the sender.
        /// </summary>
        public static void SendChatMessage(string message)
        {
            if (!ModBase.IsDedicated)
            {
                try
                {
                    MyAPIGateway.Utilities.ShowMessage(ModName, message);
                }
                catch { }
            }
        }

        /// <summary>
        /// Stops clients from updating
        /// </summary>
        private void PauseClients()
        {
            for (int n = 0; n < clients.Count; n++)
                clients[n].CanUpdate = false;

            ClientsPaused = true;
        }

        /// <summary>
        /// Allows clients to resume updating
        /// </summary>
        private void UnpauseClients()
        {
            for (int n = 0; n < clients.Count; n++)
                clients[n].CanUpdate = true;

            ClientsPaused = false;
        }

        /// <summary>
        /// Restarts all registered clients
        /// </summary>
        private void ReloadClients()
        {
            Reloading = true;

            for (int n = 0; n < clients.Count; n++)
            {
                if (clients[n].Loaded && clients[n].CanUpdate)
                    Run(clients[n].BeforeClose);
            }

            for (int n = 0; n < clients.Count; n++)
                clients[n].Close();

            for (int n = 0; n < clients.Count; n++)
                clients[n].ManualStart();

            ClientsPaused = false;
            Reloading = false;
        }

        /// <summary>
        /// Unloads all registered clients
        /// </summary>
        private void UnloadClients()
        {
            Unloading = true;

            for (int n = 0; n < clients.Count; n++)
            {
                if (clients[n].Loaded && clients[n].CanUpdate)
                    Run(clients[n].BeforeClose);
            }

            ClientsPaused = true;

            for (int n = 0; n < clients.Count; n++)
                clients[n].Close();
        }

        protected override void UnloadData()
        {
            UnloadClients();
            instance = null;
        }
    }
}
