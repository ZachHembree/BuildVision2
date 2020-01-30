using RichHudFramework.IO;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace RichHudFramework.Game
{
    /// <summary>
    /// Extends <see cref="MySessionComponentBase"/> to include built-in exception handling / logging and to allow 
    /// for types that need to be associated with and updated alongside the mod deriving from this type but without
    /// being a session component. Only one instance of <see cref="ModBase"/> can exist at a given time.
    /// </summary>
    public abstract class ModBase : MySessionComponentBase
    {
        /// <summary>
        /// Sets the mod name to be used in chat messages, popups and anything else that might require it.
        /// </summary>
        public static string ModName { get; protected set; }

        /// <summary>
        /// Sets the name of the log file to be created in the mod's local storage. Should end in .txt.
        /// </summary>
        public static string LogFileName { get; protected set; }

        /// <summary>
        /// Determines whether or not the main class will be allowed to run on a dedicated server.
        /// </summary>
        public static bool RunOnServer { get; protected set; }
        public static bool RunOnClient { get; protected set; }
        public static bool IsClient => !IsDedicated;
        public static bool IsDedicated { get; private set; }
        public static bool Unloading { get; private set; }
        public static bool NormalExit { get; protected set; }

        protected static ModBase Instance { get; private set; }
        protected MyObjectBuilder_SessionComponent SessionComponent { get; private set; }

        /// <summary>
        /// If set to true, the user will be given the option to reload in the event of an
        /// unhandled exception.
        /// </summary>
        protected static bool promptForReload;

        /// <summary>
        /// The maximum number of times the mod will be allowed to reload as a result of an unhandled exception.
        /// </summary>
        protected static int recoveryLimit;

        private static Action lastMissionScreen;
        private static int recoveryAttempts, exceptionCount;
        private const long errorLoopThreshold = 100, exceptionLimit = 10;

        private readonly List<ComponentBase> clientComponents, serverComponents;
        private readonly List<string> exceptionMessages;
        private readonly Utils.Stopwatch errorTimer;
        private bool canUpdate;
        protected LogIO log;

        static ModBase()
        {
            LogFileName = "modLog.txt";
            recoveryLimit = 1;
        }

        public ModBase(bool runOnServer, bool runOnClient)
        {
            if (Instance != null)
                throw new Exception("Only one instance of type ModBase can exist at a given time.");

            if (ModName == null)
                ModName = DebugName;

            clientComponents = new List<ComponentBase>();
            serverComponents = new List<ComponentBase>();
            RunOnServer = runOnServer;
            RunOnClient = runOnClient;

            exceptionMessages = new List<string>();
            errorTimer = new Utils.Stopwatch();
        }

        public sealed override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (Instance == null)
            {
                Instance = this;
                SessionComponent = sessionComponent;

                NormalExit = false;
                Unloading = false;
                exceptionCount = 0;
                log = new LogIO(LogFileName);

                bool isServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                IsDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);
                canUpdate = (RunOnClient && IsClient) || (RunOnServer && IsDedicated);

                if (canUpdate)
                    RunSafeAction(AfterInit);
            }
        }

        protected virtual void AfterInit() { }

        public override void Draw()
        {
            RunSafeAction(() =>
            {
                for (int n = 0; n < serverComponents.Count; n++)
                    serverComponents[n].Draw();

                for (int n = 0; n < clientComponents.Count; n++)
                    clientComponents[n].Draw();
            });
        }

        public override void HandleInput()
        {
            RunSafeAction(() =>
            {
                for (int n = 0; n < serverComponents.Count; n++)
                    serverComponents[n].HandleInput();

                for (int n = 0; n < clientComponents.Count; n++)
                    clientComponents[n].HandleInput();
            });
        }

        public sealed override void UpdateBeforeSimulation() =>
            BeforeUpdate();

        public sealed override void Simulate() =>
            BeforeUpdate();

        public sealed override void UpdateAfterSimulation() =>
            BeforeUpdate();

        /// <summary>
        /// The update function used (Before/Sim/After) is determined by the settings used by
        /// the MySessionComponentDescriptorAttribute applied to the child class.
        /// </summary>
        private void BeforeUpdate()
        {
            if (Instance == null && !Unloading)
                Init(null);

            RunSafeAction(UpdateComponents);

            if (exceptionMessages.Count > 0 && errorTimer.ElapsedMilliseconds > errorLoopThreshold)
            {
                string exceptionText = GetExceptionMessages();

                TryWriteToLog(ModName + " encountered an unhandled exception.\n" + exceptionText);
                exceptionMessages.Clear();

                Close();

                if (!IsDedicated && promptForReload)
                {
                    if (recoveryAttempts < recoveryLimit)
                    {
                        ShowErrorPrompt(exceptionText, true);
                        Unloading = true;
                    }
                    else
                        ShowErrorPrompt(exceptionText, false);
                }

                recoveryAttempts++;
            }

            // This is a workaround. If you try to create a mission screen while the chat is open, 
            // the UI will become unresponsive.
            if (lastMissionScreen != null && !MyAPIGateway.Gui.ChatEntryVisible)
            {
                lastMissionScreen();
                lastMissionScreen = null;
            }
        }

        private void UpdateComponents()
        {
            for (int n = 0; n < serverComponents.Count; n++)
                serverComponents[n].Update();

            for (int n = 0; n < clientComponents.Count; n++)
                clientComponents[n].Update();

            if (canUpdate)
                Update();
        }

        protected virtual void Update() { }

        /// <summary>
        /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it, display an error message to the user then unload the mod and its components.
        /// </summary>
        public static void RunSafeAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                ReportException(e);
            }
        }

        /// <summary>
        /// Attempts to report and log a given <see cref="Exception"/>.
        /// </summary>
        public static void ReportException(Exception e)
        {
            if (Instance != null && !Unloading)
                Instance.HandleException(e);
        }

        private void HandleException(Exception e)
        {
            string message = e.ToString();

            if (!exceptionMessages.Contains(message))
                exceptionMessages.Add(message);

            exceptionCount++;
            errorTimer.Start();
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

        private string GetExceptionMessages()
        {
            StringBuilder errorMessage = new StringBuilder();

            if (exceptionCount > exceptionLimit && errorTimer.ElapsedMilliseconds < errorLoopThreshold)
                errorMessage.AppendLine($"[Exception Loop Detected] {exceptionCount} exceptions were reported within a span of {errorTimer.ElapsedMilliseconds}ms.");

            foreach (string msg in exceptionMessages)
                errorMessage.Append(msg);

            errorMessage.Replace("--->", "\n   --->");
            return errorMessage.ToString();
        }

        private void AllowReload(ResultEnum response)
        {
            if (response == ResultEnum.OK)
                Unloading = false;
            else
                Unloading = true;
        }

        /// <summary>
        /// Attempts to synchronously update mod log with message and adds a time stamp.
        /// </summary>
        public static void TryWriteToLog(string message)
        {
            if (Instance != null && Instance.log != null)
                Instance.log.TryWriteToLog(message);
        }

        /// <summary>
        /// Attempts to update mod log in parallel with message and adds a time stamp.
        /// </summary>
        public static void WriteToLogStart(string message)
        {
            if (Instance != null && Instance.log != null)
                Instance.log.WriteToLogStart(message);
        }

        /// <summary>
        /// Sends chat message using the mod name as the sender.
        /// </summary>
        public static void SendChatMessage(string message)
        {
            if (Instance != null && !Unloading && !IsDedicated)
                MyAPIGateway.Utilities.ShowMessage(ModName, message);
        }

        /// <summary>
        /// Creates a message window using the mod name, a given subheading and a message.
        /// </summary>
        public static void ShowMessageScreen(string subHeading, string message)
        {
            if (Instance != null && !Unloading && !IsDedicated)
                ShowMissionScreen(subHeading, message, null, "Close");
        }

        private static void ShowMissionScreen(string subHeading = null, string message = null, Action<ResultEnum> callback = null, string okButtonCaption = null)
        {
            Action messageAction = () => MyAPIGateway.Utilities.ShowMissionScreen(ModName, subHeading, null, message, callback, okButtonCaption);
            lastMissionScreen = messageAction;
        }

        protected override void UnloadData()
        {
            if (!Unloading)
            {
                NormalExit = true;
                Close();
                Unloading = true;
            }
        }

        public void Reload()
        {
            if (!Unloading)
            {
                NormalExit = true;
                Close();
            }
        }

        protected virtual void BeforeClose() { }

        public void Close()
        {
            if (Instance != null && !Unloading)
            {
                Unloading = true;

                if (canUpdate)
                {
                    RunSafeAction(Instance.BeforeClose);

                    for (int n = 0; n < clientComponents.Count; n++)
                        RunSafeAction(clientComponents[n].Close);

                    for (int n = 0; n < serverComponents.Count; n++)
                        RunSafeAction(serverComponents[n].Close);
                }

                clientComponents.Clear();
                serverComponents.Clear();
                Instance = null;

                if (recoveryAttempts < recoveryLimit)
                    Unloading = false;
            }
        }

        /// <summary>
        /// Base for classes that need to be continuously updated by the game. This should not be used for short
        /// lived objects.
        /// </summary>
        public abstract class ComponentBase
        {
            /// <summary>
            /// Determines whether or not this component will run on a dedicated server.
            /// </summary>
            public readonly bool runOnServer, runOnClient;
            private readonly int index;

            protected ComponentBase(bool runOnServer, bool runOnClient)
            {
                if (Instance == null)
                    throw new Exception("Types of ComponentBase cannot be instantiated before ModBase.");

                this.runOnServer = runOnServer;
                this.runOnClient = runOnClient;

                if (!IsDedicated && runOnClient)
                {
                    index = Instance.clientComponents.Count;
                    Instance.clientComponents.Add(this);
                }
                else if (IsDedicated && runOnServer)
                {
                    index = Instance.serverComponents.Count;
                    Instance.serverComponents.Add(this);
                }
            }

            /// <summary>
            /// Used to manually remove object from update queue. This should only be used for objects that
            /// need to be closed while the mod is running.
            /// </summary>
            public virtual void UnregisterComponent()
            {
                if (!IsDedicated && runOnClient)
                    Instance.clientComponents.RemoveAt(index);
                else if (IsDedicated && runOnServer)
                    Instance.serverComponents.RemoveAt(index);
            }

            public virtual void Draw() { }

            public virtual void HandleInput() { }

            public virtual void Update() { }

            public virtual void Close() { }
        }

        /// <summary>
        /// Extension of <see cref="ComponentBase"/> that includes a task pool.
        /// </summary>
        public abstract class ParallelComponentBase : ComponentBase
        {
            private readonly TaskPool taskPool;

            protected ParallelComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient)
            {
                taskPool = new TaskPool(ErrorCallback);
            }

            /// <summary>
            /// Called in the event an exception occurs in one of the component's tasks with a list of <see cref="KnownException"/>s
            /// and a single aggregate exception of all other exceptions.
            /// </summary>
            protected abstract void ErrorCallback(List<KnownException> knownExceptions, AggregateException aggregate);

            /// <summary>
            /// Enqueues an action to run in parallel. Not thread safe; must be called from the main thread.
            /// </summary>
            protected void EnqueueTask(Action action) =>
                taskPool.EnqueueTask(action);

            /// <summary>
            /// Enqueues an action to run on the main thread. Meant to be used by threads other than the main.
            /// </summary>
            protected void EnqueueAction(Action action) =>
                taskPool.EnqueueAction(action);
        }
    }
}
