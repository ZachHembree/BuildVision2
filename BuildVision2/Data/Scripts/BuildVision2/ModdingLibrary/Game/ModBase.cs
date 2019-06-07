using DarkHelmet.IO;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
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
        public static bool RunOnServer { get; protected set; }

        protected static ModBase Instance { get; private set; }
        protected bool Crashed { get; private set; }
        protected bool IsDedicated { get; private set; }
        protected bool IsServer { get; private set; }
        protected bool Closing { get; private set; }

        private static readonly List<Action> closeActions, drawActions, inputActions, updateActions;

        static ModBase()
        {
            ModName = "Mod Base";
            LogIO.FileName = "modLog.txt";
            TaskPool.MaxTasksRunning = 2;
            RunOnServer = false;

            closeActions = new List<Action>();
            drawActions = new List<Action>();
            inputActions = new List<Action>();
            updateActions = new List<Action>();
        }

        public ModBase()
        {
            if (Instance != null)
                throw new Exception("Only one instance of type ModBase can exist at a given time.");

            Crashed = false;
            IsDedicated = false;
            IsServer = false;
            Closing = false;
        }

        public sealed override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (Instance == null)
            {
                Instance = this;
                Closing = false;
                IsServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                IsDedicated = (MyAPIGateway.Utilities.IsDedicated && IsServer);

                RunSafeAction(AfterInit);
            }
        }

        protected virtual void AfterInit() { }

        public sealed override void Draw() =>
            UpdateComponents(drawActions);

        public sealed override void HandleInput() =>
            UpdateComponents(inputActions);

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
            UpdateComponents(updateActions);
            RunSafeAction(Update);
        }

        protected virtual void Update() { }

        private void UpdateComponents(List<Action> updateActions)
        {
            RunSafeAction(() =>
            {
                for (int n = 0; n < updateActions.Count; n++)
                    updateActions[n]();
            });
        }

        /// <summary>
        /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it, display an error message to the user then unload the mod and its components.
        /// </summary>
        public static void RunSafeAction(Action action)
        {
            if (Instance != null && !Instance.Crashed && (!Instance.IsDedicated || RunOnServer))
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Instance.Crashed = true;

                    if (!Instance.Closing)
                    {
                        LogIO.Instance.TryWriteToLog(ModName + " has crashed!\n" + e.ToString());

                        MyAPIGateway.Utilities.ShowMissionScreen
                        (
                            ModName, "Debug", "",
                            $"{ModName} has encountered a problem and will attempt to reload. Press the X in the upper right hand corner " +
                            "to cancel.\n" + e.ToString(),
                            Instance.AllowReload, "Reload"
                        );

                        Close();
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
        /// Sends chat message using the mod name as the sender.
        /// </summary>
        public static void SendChatMessage(string message)
        {
            if (Instance != null && !Instance.Closing)
                MyAPIGateway.Utilities.ShowMessage(ModName, message);
        }

        /// <summary>
        /// Creates a message window using the mod name, a given subheading and a message.
        /// </summary>
        public static void ShowMessageScreen(string subHeading, string message)
        {
            if (Instance != null && !Instance.Closing)
                MyAPIGateway.Utilities.ShowMissionScreen(ModName, subHeading, null, message, null, "Close");
        }

        protected override void UnloadData()
        {
            Closing = true;
            Close();
        }

        protected virtual void BeforeClose() { }

        public static void Close()
        {
            RunSafeAction(() =>
            {
                foreach (Action CloseAction in closeActions)
                    CloseAction();

                Instance.BeforeClose();
                drawActions.Clear();
                inputActions.Clear();
                updateActions.Clear();
                closeActions.Clear();
                Instance = null;
            });
        }

        public abstract class Component
        {
            protected Component()
            {
                drawActions.Add(Draw);
                inputActions.Add(HandleInput);
                updateActions.Add(Update);
                closeActions.Add(Close);
            }

            protected virtual void Draw() { }

            protected virtual void HandleInput() { }

            protected virtual void Update() { }

            protected virtual void BeforeClose() { }

            public virtual void Close()
            {
                BeforeClose();
                drawActions.Remove(Draw);
                inputActions.Remove(HandleInput);
                updateActions.Remove(Update);
                closeActions.Remove(Close);
            }
        }

        public abstract class SingletonComponent<T> : Singleton<T> where T : SingletonComponent<T>, new()
        {
            static SingletonComponent()
            {
                drawActions.Add(() => Instance?.Draw());
                inputActions.Add(() => Instance?.HandleInput());
                updateActions.Add(() => Instance?.Update());
                closeActions.Add(Close);
            }

            protected virtual void Draw() { }

            protected virtual void HandleInput() { }

            protected virtual void Update() { }
        }

        public abstract class ParallelComponent<T> : SingletonComponent<T> where T : ParallelComponent<T>, new()
        {
            private TaskPool taskPool;

            static ParallelComponent()
            {
                updateActions.Add(() => Instance?.taskPool?.Update());
            }

            protected ParallelComponent()
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
