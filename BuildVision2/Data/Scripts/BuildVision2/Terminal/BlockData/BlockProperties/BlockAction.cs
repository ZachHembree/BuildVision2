using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;
using IMyTerminalAction = Sandbox.ModAPI.Interfaces.ITerminalAction;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Custom block actions
        /// </summary>
        private class BlockAction : BlockMemberBase, IBlockAction
        {
            public override string Value => GetValueFunc();
            public override string Postfix => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private readonly Func<string> GetValueFunc, GetPostfixFunc;
            private readonly Action action;

            public BlockAction(Func<string> GetValueFunc, Func<string> GetPostfixFunc, Action Action)
            {
                Name = null;
                Enabled = true;

                this.GetValueFunc = GetValueFunc;
                this.GetPostfixFunc = GetPostfixFunc;
                action = Action;
            }

            public BlockAction(string value, Func<string> GetPostfixFunc, Action Action)
                : this(() => value, GetPostfixFunc, Action) { }

            public void Action() =>
                action();

            /// <summary>
            /// Gets actions for blocks implementing IMyMechanicalConnectionBlock.
            /// </summary>
            public static void GetMechActions(SuperBlock blockData, List<IBlockMember> members)
            {
                List<IMyTerminalAction> terminalActions = new List<IMyTerminalAction>();
                blockData.TBlock.GetActions(terminalActions);

                if (blockData.SubtypeId.HasFlag(TBlockSubtypes.Suspension))
                {
                    members.Add(new BlockAction(
                        "Attach Wheel",
                        () => blockData.MechConnection.PartAttached ? "(Attached)" : null,
                        blockData.MechConnection.AttachHead));
                    members.Add(new BlockAction(
                        "Detach Wheel", null,
                        blockData.MechConnection.DetachHead));
                }
                else
                {
                    members.Add(new BlockAction(
                        "Attach Head",
                        () => blockData.MechConnection.PartAttached ? "(Attached)" : null,
                        blockData.MechConnection.AttachHead));
                    members.Add(new BlockAction(
                        "Detach Head", null,
                        blockData.MechConnection.DetachHead));
                }

                foreach (IMyTerminalAction tAction in terminalActions)
                {
                    if (tAction.Id.StartsWith("Add"))
                    {
                        members.Add(new BlockAction(
                            tAction.Name.ToString(), null,
                            () => tAction.Apply(blockData.TBlock)));
                    }
                }

                if (blockData.SubtypeId.HasFlag(TBlockSubtypes.Piston))
                {
                    members.Add(new BlockAction(
                        "Reverse", null,
                         blockData.Piston.Reverse));
                }
                else if (blockData.SubtypeId.HasFlag(TBlockSubtypes.Rotor))
                {
                    members.Add(new BlockAction(
                        "Reverse", null,
                        blockData.Rotor.Reverse));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyDoor.
            /// </summary>
            public static void GetDoorActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Open/Close", null,
                    blockData.Door.ToggleDoor));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Start Countdown",
                    () => $"({ Math.Truncate(blockData.Warhead.CountdownTime) })",
                    () => blockData.Warhead.StartCountdown()));
                members.Add(new BlockAction(
                    "Stop Countdown", null,
                    () => blockData.Warhead.StopCountdown()));
                members.Add(new BlockAction(
                    "Detonate", null,
                    blockData.Warhead.Detonate));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyLandingGear.
            /// </summary>
            public static void GetGearActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Lock/Unlock",
                    () =>
                    {
                        string status = "";

                        if (blockData.LandingGear.Status == LandingGearMode.Locked)
                            status = "(Locked)";
                        else if (blockData.LandingGear.Status == LandingGearMode.ReadyToLock)
                            status = "(Ready)";
                        else if (blockData.LandingGear.Status == LandingGearMode.Unlocked)
                            status = "(Unlocked)";

                        return status;
                    },
                    blockData.LandingGear.ToggleLock));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetConnectorActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Lock/Unlock",
                    () =>
                    {
                        string status = "";

                        if (blockData.Connector.Status == ConnectorStatus.Connected)
                            status = "(Locked)";
                        else if (blockData.Connector.Status == ConnectorStatus.Connectable)
                            status = "(Ready)";
                        else if (blockData.Connector.Status == ConnectorStatus.Unconnected)
                            status = "(Unlocked)";

                        return status;
                    },
                    blockData.Connector.ToggleConnect));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyParachute.
            /// </summary>
            public static void GetChuteActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Open/Close",
                    () => $"({blockData.Door.Status})",
                    blockData.Door.ToggleDoor));
            }
        }
    }
}