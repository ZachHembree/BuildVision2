using RichHudFramework.IO;
using RichHudFramework.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace RichHudFramework
{
    /// <summary>
    /// Handles exceptions. Shocking, right?
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0)]
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

        public static int RecoveryAttempts { get; private set; }

        /// <summary>
        /// If set to true, the user will be given the option to reload in the event of an
        /// unhandled exception.
        /// </summary>
        public static bool PromptForReload { get; set; }

        public static bool Reloading { get; private set; }

        public static bool Unloading { get; private set; }

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

        private void ReportException(Exception e)
        {
            string message = e.ToString();

            if (!exceptionMessages.Contains(message))
                exceptionMessages.Add(message);

            exceptionCount++;
            errorTimer.Start();

            // Exception loop, respond immediately
            if (exceptionCount > exceptionLoopCount && errorTimer.ElapsedMilliseconds < exceptionLoopTime)
                HandleExceptions();
        }

        public override void UpdateBeforeSimulation()
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

        private void HandleExceptions()
        {
            string exceptionText = GetExceptionMessages();
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

        private string GetExceptionMessages()
        {
            StringBuilder errorMessage = new StringBuilder();

            if (exceptionCount > exceptionLoopCount && errorTimer.ElapsedMilliseconds < exceptionLoopTime)
                errorMessage.AppendLine($"[Exception Loop Detected] {exceptionCount} exceptions were reported within a span of {errorTimer.ElapsedMilliseconds}ms.");

            foreach (string msg in exceptionMessages)
                errorMessage.Append(msg);

            errorMessage.Replace("--->", "\n   --->");
            return errorMessage.ToString();
        }

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

        private void AllowReload(ResultEnum response)
        {
            if (response == ResultEnum.OK)
                ReloadClients();
            else
                UnloadClients();
        }

        public static void ShowMissionScreen(string subHeading = null, string message = null, Action<ResultEnum> callback = null, string okButtonCaption = null)
        {
            if (!Unloading && instance.Loaded)
            {
                Action messageAction = () => MyAPIGateway.Utilities.ShowMissionScreen(ModName, subHeading, null, message, callback, okButtonCaption);
                instance.lastMissionScreen = messageAction;
            }
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
            if (!Unloading && !ModBase.IsDedicated)
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
        }

        /// <summary>
        /// Allows clients to resume updating
        /// </summary>
        private void UnpauseClients()
        {
            for (int n = 0; n < clients.Count; n++)
                clients[n].CanUpdate = true;
        }

        /// <summary>
        /// Closes all registered clients
        /// </summary>
        private void CloseClients()
        {
            for (int n = 0; n < clients.Count; n++)
                clients[n].Close();
        }

        /// <summary>
        /// Restarts all registered clients
        /// </summary>
        private void ReloadClients()
        {
            Reloading = true;

            for (int n = 0; n < clients.Count; n++)
                clients[n].Reload();

            Reloading = false;
        }

        /// <summary>
        /// Unloads all registered clients
        /// </summary>
        private void UnloadClients()
        {
            for (int n = clients.Count - 1; n >= 0; n--)
                clients[n].Unload();
        }

        protected override void UnloadData()
        {
            Unloading = true;
            instance = null;
        }
    }
}
