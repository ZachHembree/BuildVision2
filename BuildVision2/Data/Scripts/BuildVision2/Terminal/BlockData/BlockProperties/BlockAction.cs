using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;
using IMyParachute = SpaceEngineers.Game.ModAPI.Ingame.IMyParachute;
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
            public static void GetMechActions(IMyMechanicalConnectionBlock mechBlock, List<IBlockMember> members)
            {
                List<IMyTerminalAction> terminalActions = new List<IMyTerminalAction>();
                mechBlock.GetActions(terminalActions);

                if (mechBlock is IMyMotorSuspension)
                {
                    members.Add(new BlockAction(
                        "Attach Wheel",
                        () => mechBlock.IsAttached ? "(Attached)" : null,
                        mechBlock.Attach));
                    members.Add(new BlockAction(
                        "Detach Wheel", null,
                        mechBlock.Detach));
                }
                else
                {
                    members.Add(new BlockAction(
                        "Attach Head",
                        () => mechBlock.IsAttached ? "(Attached)" : null,
                        mechBlock.Attach));
                    members.Add(new BlockAction(
                        "Detach Head", null,
                        mechBlock.Detach));
                }

                foreach (IMyTerminalAction tAction in terminalActions)
                {
                    string tActionName = tAction.Name.ToString();

                    if (tAction.Id.StartsWith("Add"))
                    {
                        members.Add(new BlockAction(
                            tActionName, null,
                            () => tAction.Apply(mechBlock)));
                    }
                }

                if (mechBlock is IMyPistonBase)
                {
                    IMyPistonBase piston = (IMyPistonBase)mechBlock;

                    members.Add(new BlockAction(
                        "Reverse", null,
                         piston.Reverse));
                }
                else if (mechBlock is IMyMotorStator)
                {
                    IMyMotorStator rotor = (IMyMotorStator)mechBlock;
                    
                    members.Add(new BlockAction(
                        "Reverse", null,
                        () => rotor.TargetVelocityRad = -rotor.TargetVelocityRad));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyDoor.
            /// </summary>
            public static void GetDoorActions(IMyDoor doorBlock, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Open/Close", null,
                    doorBlock.ToggleDoor));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(IMyWarhead warhead, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Start Countdown",
                    () => $"({ Math.Truncate(warhead.DetonationTime) })",
                    () => warhead.StartCountdown()));
                members.Add(new BlockAction(
                    "Stop Countdown", null,
                    () => warhead.StopCountdown()));
                members.Add(new BlockAction(
                    "Detonate", null,
                    warhead.Detonate));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyLandingGear.
            /// </summary>
            public static void GetGearActions(IMyLandingGear landingGear, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Lock/Unlock",
                    () =>
                    {
                        string status = "";

                        if (landingGear.LockMode == LandingGearMode.Locked)
                            status = "(Locked)";
                        else if (landingGear.LockMode == LandingGearMode.ReadyToLock)
                            status = "(Ready)";
                        else if (landingGear.LockMode == LandingGearMode.Unlocked)
                            status = "(Unlocked)";

                        return status;
                    },
                    landingGear.ToggleLock));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetConnectorActions(IMyShipConnector connector, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Lock/Unlock",
                    () =>
                    {
                        string status = "";

                        if (connector.Status == ConnectorStatus.Connected)
                            status = "(Locked)";
                        else if (connector.Status == ConnectorStatus.Connectable)
                            status = "(Ready)";
                        else if (connector.Status == ConnectorStatus.Unconnected)
                            status = "(Unlocked)";

                        return status;
                    },
                    connector.ToggleConnect));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyParachute.
            /// </summary>
            public static void GetChuteActions(IMyParachute parachute, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    "Open/Close",
                    () => $"({parachute.Status.ToString()})",
                    parachute.ToggleDoor));
            }
        }
    }
}