using DarkHelmet.IO;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace DarkHelmet.Game
{
    /// <summary>
    /// Base class for mods using this library. 
    /// </summary>
    public abstract class ModBase : MySessionComponentBase
    {
        public static string ModName { get; protected set; }
        public static string LogFileName { get; protected set; }
        public static bool RunOnServer { get; protected set; }
        public static bool IsDedicated { get; private set; }
        public static bool IsServer { get; private set; }

        protected static ModBase Instance { get; private set; }
        protected static bool Unload { get; private set; }
        protected bool Crashed { get; private set; }

        private LogIO log;
        protected List<ComponentBase> modComponents;

        static ModBase()
        {
            ModName = "Mod Base";
            LogFileName = "modLog.txt";
            RunOnServer = false;            
        }

        public ModBase()
        {
            if (Instance != null)
                throw new Exception("Only one instance of type ModBase can exist at a given time.");            
        }

        public sealed override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (Instance == null)
            {
                Instance = this;
                Unload = false;
                IsServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                IsDedicated = (MyAPIGateway.Utilities.IsDedicated && IsServer);
                modComponents = new List<ComponentBase>(10);

                log = new LogIO(LogFileName);
                RunSafeAction(AfterInit);
            }
        }

        protected virtual void AfterInit() { }

        public sealed override void Draw()
        {
            if (Instance == null && !Unload)
                Init(null);

            RunSafeAction(() =>
            {
                for (int n = 0; n < modComponents.Count; n++)
                    modComponents[n].Draw();
            });
        }

        public sealed override void HandleInput()
        {
            if (Instance == null && !Unload)
                Init(null);

            RunSafeAction(() =>
            {
                for (int n = 0; n < modComponents.Count; n++)
                    modComponents[n].HandleInput();
            });
        }

        public sealed override void UpdateBeforeSimulation() =>
            RunSafeAction(BeforeUpdate);

        public sealed override void Simulate() =>
            RunSafeAction(BeforeUpdate);

        public sealed override void UpdateAfterSimulation() =>
            RunSafeAction(BeforeUpdate);

        /// <summary>
        /// The update function used (Before/Sim/After) is determined by the settings used by
        /// the MySessionComponentDescriptorAttribute applied to the child class.
        /// </summary>
        private void BeforeUpdate()
        {
            if (Instance == null && !Unload)
                Init(null);

            UpdateComponents();
            RunSafeAction(Update);
        }

        protected virtual void Update() { }

        private void UpdateComponents()
        {
            RunSafeAction(() =>
            {
                for (int n = 0; n < modComponents.Count; n++)
                    modComponents[n].Update();
            });
        }

        /// <summary>
        /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it, display an error message to the user then unload the mod and its components.
        /// </summary>
        public static void RunSafeAction(Action action)
        {
            if (Instance != null && !Instance.Crashed && (!IsDedicated || RunOnServer))
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Instance.Crashed = true;

                    if (!Unload)
                    {
                        StringBuilder errorMessage = new StringBuilder(e.ToString());
                        errorMessage.Replace(" --->", "\n   --->");

                        TryWriteToLog(ModName + " crashed!\n" + errorMessage);

                        MyAPIGateway.Utilities.ShowMissionScreen
                        (
                            ModName, "Debug", "",
                            $"{ModName} has encountered a problem and will attempt to reload. Press the X in the upper right hand corner " +
                            "to cancel.\n\n" + 
                            "Error Details:\n" +
                            errorMessage,
                            Instance.AllowReload, 
                            "Reload"
                        );

                        Instance.Close();
                    }
                }
            }
        }

        private void AllowReload(ResultEnum response)
        {
            if (response == ResultEnum.OK)
                Crashed = false;
            else
                Crashed = true;
        }

        /// <summary>
        /// Attempts to synchronously update mod log with message and adds a time stamp.
        /// </summary>
        public static void TryWriteToLog(string message)
        {
            if (Instance != null && !Unload && Instance.log != null)
                Instance.log.TryWriteToLog(message);
        }

        /// <summary>
        /// Attempts to update mod log in parallel with message and adds a time stamp.
        /// </summary>
        public static void WriteToLogStart(string message)
        {
            if (Instance != null && !Unload && Instance.log != null)
                Instance.log.WriteToLogStart(message);
        }

        /// <summary>
        /// Sends chat message using the mod name as the sender.
        /// </summary>
        public static void SendChatMessage(string message)
        {
            if (Instance != null && !Unload)
                MyAPIGateway.Utilities.ShowMessage(ModName, message);
        }

        /// <summary>
        /// Creates a message window using the mod name, a given subheading and a message.
        /// </summary>
        public static void ShowMessageScreen(string subHeading, string message)
        {
            if (Instance != null && !Unload)
                MyAPIGateway.Utilities.ShowMissionScreen(ModName, subHeading, null, message, null, "Close");
        }

        protected override void UnloadData()
        {
            Close();
            Unload = true;
        }

        protected virtual void BeforeClose() { }

        public void Close()
        {
            if (Instance != null && !Unload)
            {
                Unload = true;
                RunSafeAction(Instance.BeforeClose);

                for (int n = 0; n < modComponents.Count; n++)
                    RunSafeAction(modComponents[n].Close);

                Instance = null;
                Unload = false;
            }
        }

        /// <summary>
        /// Base for classes that need to be continuously updated by the game. This should not be used for short
        /// lived objects.
        /// </summary>
        public abstract class ComponentBase
        {
            protected ComponentBase()
            {
                Instance.modComponents.Add(this);
            }

            /// <summary>
            /// Used to manually remove object from update queue. This should only be used for objects that
            /// need to be closed while the mod is running.
            /// </summary>
            public virtual void UnregisterComponent()
            {
                Instance.modComponents.Remove(this);
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
            private TaskPool taskPool;

            protected ParallelComponentBase()
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
