using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.ModAPI;
using VRage.Game.Components;
using RichHudFramework;
using ProtoBuf;

namespace DarkHelmet.BuildVision2
{
    public enum EntitySubtypes : long
    {
        None = 0,
        MyMechanicalConnection = 1
    }

    public enum MechBlockActions
    {
        Attach = 1,
        Detach = 2
    }

    [ProtoContract]
    public struct BlockActionMsg
    {
        [ProtoMember(1)]
        public long entID;

        [ProtoMember(2)]
        public EntitySubtypes subtypeID;

        [ProtoMember(3)]
        public MechBlockActions actionID;

        public BlockActionMsg(long entID, EntitySubtypes subtypeID, MechBlockActions actionID)
        {
            this.entID = entID;
            this.subtypeID = subtypeID;
            this.actionID = actionID;
        }
    }

    /// <summary>
    /// Build Vision main class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, -1)]
    public sealed partial class BvMain : ModBase
    {
        public static BvMain Instance { get; private set; }
        public static BvConfig Cfg => BvConfig.Current;

        private const ushort handlerID = 16971;
        private readonly List<BlockActionMsg> serverMessages;
        private readonly List<byte[]> messageData;

        public BvMain() : base(true, true)
        {
            if (Instance == null)
                Instance = this;
            else
                throw new Exception("Only one instance of BvMain can exist at any given time.");

            LogIO.FileName = "bvLog.txt";
            BvConfig.FileName = "BuildVision2Config.xml";

            ExceptionHandler.ModName = "Build Vision";
            ExceptionHandler.PromptForReload = true;
            ExceptionHandler.RecoveryLimit = 3;

            serverMessages = new List<BlockActionMsg>();
            messageData = new List<byte[]>();
        }

        /// <summary>
        /// Runs block action on the server as defined by the <see cref="BlockActionMsg"/>
        /// </summary>
        public static void SendBlockActionToServer(BlockActionMsg message)
        {
            Instance.serverMessages.Add(message);
        }

        protected override void AfterInit()
        {
            if (IsClient)
            {
                CanUpdate = false;
                RichHudClient.Init(ExceptionHandler.ModName, HudInit, Reload);
            }
            else
            {
                MyAPIGateway.Multiplayer.RegisterMessageHandler(handlerID, ServerMessageHandler);
            }
        }

        private void HudInit()
        {
            if (IsClient)
            {
                CanUpdate = true;

                BvConfig.Load(true);
                AddChatCommands();
                InitSettingsMenu();
                PropertiesMenu.Init();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (CanUpdate)
            {
                if (IsClient)
                {
                    SendServerMessages();
                }
                
                if (IsServer)
                {
                    ReceiveServerMessages();
                }
            }
        }

        private void ServerMessageHandler(byte[] message)
        {
            messageData.Add(message);
        }

        private void SendServerMessages()
        {
            if (serverMessages.Count > 0)
            {
                ExceptionHandler.WriteLineAndConsole($"Sending {serverMessages.Count} message(s) to server.");

                byte[] protoMessages;
                KnownException exception = Utils.ProtoBuf.TrySerialize(serverMessages, out protoMessages);

                if (exception == null)
                    MyAPIGateway.Multiplayer.SendMessageToServer(handlerID, protoMessages);
                else
                    ExceptionHandler.WriteLineAndConsole($"Unable to serialize server message: {exception.ToString()}");
            }

            if (!IsServer)
                serverMessages.Clear();
        }

        private void ReceiveServerMessages()
        {
            int errCount = 0;

            for (int n = 0; n < messageData.Count; n++)
            {
                List<BlockActionMsg> clientActions = new List<BlockActionMsg>();
                KnownException exception = Utils.ProtoBuf.TryDeserialize(messageData[n], out clientActions);

                if (exception != null)
                    errCount++;
                else
                {
                    serverMessages.AddRange(clientActions);
                }
            }

            if (errCount < serverMessages.Count)
            {
                ExceptionHandler.WriteLineAndConsole($"Recieved {serverMessages.Count} message(s) from client(s).");

                for (int n = 0; n < serverMessages.Count; n++)
                {
                    IMyEntity entity = MyAPIGateway.Entities.GetEntityById(serverMessages[n].entID);

                    switch ((EntitySubtypes)serverMessages[n].subtypeID)
                    {
                        case EntitySubtypes.None:
                            break;
                        case EntitySubtypes.MyMechanicalConnection:
                            {
                                var mechBlock = entity as IMyMechanicalConnectionBlock;

                                if (mechBlock != null)
                                {
                                    switch (serverMessages[n].actionID)
                                    {
                                        case MechBlockActions.Attach:
                                            mechBlock.Attach(); break;
                                        case MechBlockActions.Detach:
                                            mechBlock.Detach(); break;
                                    }
                                }

                                break;
                            }
                    }
                }
            }
            else if (errCount > 0)
            {
                ExceptionHandler.WriteLineAndConsole($"Unable to parse {errCount} of {serverMessages.Count} client message(s).");
            }

            messageData.Clear();
            serverMessages.Clear();
        }

        public override void BeforeClose()
        {
            if (IsClient)
            {
                BvConfig.Save();

                if (!ExceptionHandler.Unloading)
                    RichHudClient.Reset();
                else
                    Instance = null;
            }
        }
    }

    public abstract class BvComponentBase : ModBase.ComponentBase
    {
        public BvComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, BvMain.Instance)
        { }
    }
}