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
    /// Extends <see cref="MySessionComponentBase"/> to include built-in exception handling, logging and a component
    /// system.
    /// </summary>
    public abstract class ModBase : MySessionComponentBase
    {
        private const long errorLoopThreshold = 100, exceptionLimit = 10;

        /// <summary>
        /// Sets the mod name to be used in chat messages, popups and anything else that might require it.
        /// </summary>
        public string ModName { get; protected set; }

        /// <summary>
        /// Determines whether or not the main class will be allowed to run on a dedicated server.
        /// </summary>
        public bool RunOnServer { get; protected set; }

        /// <summary>
        /// If true, then the mod will be allowed to run on a client.
        /// </summary>
        public bool RunOnClient { get; protected set; }

        /// <summary>
        /// If true, the mod is currently running on a client.
        /// </summary>
        public static bool IsClient => !IsDedicated;

        /// <summary>
        /// If true, the mod is currently running on a dedicated server.
        /// </summary>
        public static bool IsDedicated { get; private set; }

        /// <summary>
        /// If true, the mod is currently loaded.
        /// </summary>
        public new bool Loaded { get; private set; }

        public bool CanLoad => !(Unloading || Reloading);

        /// <summary>
        /// If true, then the mod is currently in the process of unloading.
        /// </summary>
        public bool Unloading { get; private set; }

        /// <summary>
        /// If true, then themod is currently in the process of reloading.
        /// </summary>
        public bool Reloading { get; private set; }

        /// <summary>
        /// If set to true, the user will be given the option to reload in the event of an
        /// unhandled exception.
        /// </summary>
        protected bool promptForReload;

        /// <summary>
        /// The maximum number of times the mod will be allowed to reload as a result of an unhandled exception.
        /// </summary>
        protected int recoveryLimit;

        private int recoveryAttempts, exceptionCount;

        private readonly List<ComponentBase> clientComponents, serverComponents;
        private readonly List<string> exceptionMessages;
        private readonly Utils.Stopwatch errorTimer;
        private bool canUpdate;
        private Action lastMissionScreen;
        protected LogIO log;

        protected ModBase(bool runOnServer, bool runOnClient)
        {
            ModName = DebugName;
            recoveryLimit = 1;

            clientComponents = new List<ComponentBase>();
            serverComponents = new List<ComponentBase>();
            RunOnServer = runOnServer;
            RunOnClient = runOnClient;
            
            exceptionMessages = new List<string>();
            errorTimer = new Utils.Stopwatch();
        }

        public sealed override void LoadData()
        {
            if (!Loaded && CanLoad)
            {
                bool isServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                IsDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);
                canUpdate = (RunOnClient && IsClient) || (RunOnServer && IsDedicated);

                Reloading = false;
                Unloading = false;
                exceptionCount = 0;

                if (canUpdate)
                    AfterLoadData();
            }
        }
        
        protected new virtual void AfterLoadData() { }

        public sealed override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (canUpdate)
                AfterInit();

            Loaded = true;
        }

        protected virtual void AfterInit() { }

        public override void Draw()
        {
            if (Loaded && canUpdate)
            {
                RunSafeAction(() =>
                {
                    for (int n = 0; n < serverComponents.Count; n++)
                        serverComponents[n].Draw();

                    for (int n = 0; n < clientComponents.Count; n++)
                        clientComponents[n].Draw();
                });
            }
        }

        public override void HandleInput()
        {
            if (Loaded && canUpdate)
            {
                RunSafeAction(() =>
                {
                    for (int n = 0; n < serverComponents.Count; n++)
                        serverComponents[n].HandleInput();

                    for (int n = 0; n < clientComponents.Count; n++)
                        clientComponents[n].HandleInput();
                });
            }
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
            if (Loaded && canUpdate)
                RunSafeAction(UpdateComponents);

            if (exceptionMessages.Count > 0 && errorTimer.ElapsedMilliseconds > errorLoopThreshold)
            {
                string exceptionText = GetExceptionMessages();

                TryWriteToLog(ModName + " encountered an unhandled exception.\n" + exceptionText);
                exceptionMessages.Clear();

                if (IsClient && promptForReload)
                    ShowErrorPrompt(exceptionText, recoveryAttempts < recoveryLimit);

                Reload();
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

        private void UpdateComponents()
        {
            for (int n = 0; n < serverComponents.Count; n++)
                serverComponents[n].Update();

            for (int n = 0; n < clientComponents.Count; n++)
                clientComponents[n].Update();

            Update();
        }

        protected virtual void Update() { }

        /// <summary>
        /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it, display an error message to the user then unload the mod and its components.
        /// </summary>
        public void RunSafeAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
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

        private void AllowReload(ResultEnum response)
        {
            if (response == ResultEnum.OK)
                Reloading = false;
            else
                Reloading = true;
        }

        /// <summary>
        /// Attempts to synchronously update mod log with message and adds a time stamp.
        /// </summary>
        public bool TryWriteToLog(string message) =>
            LogIO.TryWriteToLog(message);

        /// <summary>
        /// Attempts to update mod log in parallel with message and adds a time stamp.
        /// </summary>
        public void WriteToLogStart(string message) =>
            LogIO.WriteToLogStart(message);

        /// <summary>
        /// Sends chat message using the mod name as the sender.
        /// </summary>
        public void SendChatMessage(string message)
        {
            if (!Unloading && !IsDedicated)
                MyAPIGateway.Utilities.ShowMessage(ModName, message);
        }

        /// <summary>
        /// Creates a message window using the mod name, a given subheading and a message.
        /// </summary>
        public void ShowMessageScreen(string subHeading, string message)
        {
            if (!Unloading && !IsDedicated)
                ShowMissionScreen(subHeading, message, null, "Close");
        }

        private void ShowMissionScreen(string subHeading = null, string message = null, Action<ResultEnum> callback = null, string okButtonCaption = null)
        {
            Action messageAction = () => MyAPIGateway.Utilities.ShowMissionScreen(ModName, subHeading, null, message, callback, okButtonCaption);
            lastMissionScreen = messageAction;
        }

        public void Reload()
        {
            if (Loaded)
            {
                if (recoveryAttempts < recoveryLimit)
                {
                    Reloading = true;
                    Close();
                    Reloading = false;

                    if (!Loaded && CanLoad)
                    {
                        RunSafeAction(() => 
                        {
                            LoadData();
                            Init(null);
                        });
                    }
                }
                else
                    UnloadData();
            }
        }

        protected override void UnloadData()
        {
            if (!Unloading)
            {
                Unloading = true;
                Close();
            }
        }

        private void Close()
        {
            if (Loaded)
            {
                Loaded = false;

                if (canUpdate)
                {
                    RunSafeAction(BeforeClose);

                    for (int n = clientComponents.Count - 1; n >= 0; n--)
                    {
                        RunSafeAction(clientComponents[n].Close);
                        clientComponents[n].UnregisterComponent(n);
                    }

                    for (int n = serverComponents.Count - 1; n >= 0; n--)
                    {
                        RunSafeAction(serverComponents[n].Close);
                        serverComponents[n].UnregisterComponent(n);
                    }
                }

                clientComponents.Clear();
                serverComponents.Clear();
            }
        }

        protected virtual void BeforeClose() { }

        /// <summary>
        /// Base class for ModBase components.
        /// </summary>
        public abstract class ComponentBase
        {
            protected ModBase Parent { get; private set; }

            /// <summary>
            /// Determines whether or not this component will run on a dedicated server.
            /// </summary>
            public readonly bool runOnServer, runOnClient;

            protected ComponentBase(bool runOnServer, bool runOnClient, ModBase parent)
            {
                this.runOnServer = runOnServer;
                this.runOnClient = runOnClient;

                RegisterComponent(parent);
            }

            public void RegisterComponent(ModBase parent)
            {
                if (Parent == null)
                {
                    if (!IsDedicated && runOnClient)
                        parent.clientComponents.Add(this);
                    else if (IsDedicated && runOnServer)
                        parent.serverComponents.Add(this);

                    Parent = parent;
                }
            }

            /// <summary>
            /// Used to manually remove object from update queue. This should only be used for objects that
            /// need to be closed while the mod is running.
            /// </summary>
            public void UnregisterComponent()
            {
                if (Parent != null)
                {
                    if (!IsDedicated && runOnClient)
                        Parent.clientComponents.Remove(this);
                    else if (IsDedicated && runOnServer)
                        Parent.serverComponents.Remove(this);

                    Parent = null;
                }
            }

            /// <summary>
            /// Used to manually remove object from update queue. This should only be used for objects that
            /// need to be closed while the mod is running.
            /// </summary>
            public void UnregisterComponent(int index)
            {
                if (Parent != null)
                {
                    if (!IsDedicated && runOnClient)
                    {
                        if (index < Parent.clientComponents.Count && Parent.clientComponents[index] == this)
                        {
                            Parent.clientComponents.RemoveAt(index);
                            Parent = null;
                        }
                    }
                    else if (IsDedicated && runOnServer)
                    {
                        if (index < Parent.serverComponents.Count && Parent.serverComponents[index] == this)
                        {
                            Parent.serverComponents.RemoveAt(index);
                            Parent = null;
                        }
                    }
                }
            }

            public virtual void Draw() { }

            public virtual void HandleInput() { }

            public virtual void Update() { }

            public virtual void Close() { }

            /// <summary>
            /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
            /// to log it, display an error message to the user then unload the mod and its components.
            /// </summary>
            protected void RunSafeAction(Action action) =>
                Parent.RunSafeAction(action);

            /// <summary>
            /// Attempts to synchronously update mod log with message and adds a time stamp.
            /// </summary>
            protected bool TryWriteToLog(string message) =>
                LogIO.TryWriteToLog(message);

            /// <summary>
            /// Attempts to update mod log in parallel with message and adds a time stamp.
            /// </summary>
            protected void WriteToLogStart(string message) =>
                LogIO.WriteToLogStart(message);

            /// <summary>
            /// Sends chat message using the mod name as the sender.
            /// </summary>
            protected void SendChatMessage(string message) =>
                Parent.SendChatMessage(message);
        }

        /// <summary>
        /// Extension of <see cref="ComponentBase"/> that includes a task pool.
        /// </summary>
        public abstract class ParallelComponentBase : ComponentBase
        {
            private readonly TaskPool taskPool;

            protected ParallelComponentBase(bool runOnServer, bool runOnClient, ModBase parent) : base(runOnServer, runOnClient, parent)
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
