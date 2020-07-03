using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage;
using RichHudFramework.Internal;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
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
            public override string Display => GetValueFunc();
            public override string Status => GetPostfixFunc != null ? GetPostfixFunc() : null;

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
            public static void GetMechActions(SuperBlock block, List<IBlockMember> members)
            {
                List<IMyTerminalAction> terminalActions = new List<IMyTerminalAction>();
                block.TBlock.GetActions(terminalActions);

                IMyTerminalAction attach = terminalActions.Find(x => x.Id == "Attach");

                if (attach != null)
                {
                    members.Add(new BlockAction(
                        MyTexts.GetString(MySpaceTexts.BlockActionTitle_Attach),
                        () => $"({block.MechConnection.GetLocalizedAttachStatus()})",
                        () => attach.Apply(block.TBlock)));
                    members.Add(new BlockAction(
                        MyTexts.GetString(MySpaceTexts.BlockActionTitle_Detach), null,
                        block.MechConnection.DetachHead));
                }

                foreach (IMyTerminalAction tAction in terminalActions)
                {
                    if (tAction.IsEnabled(block.TBlock) && tAction.Id.StartsWith("Add"))
                    {
                        members.Add(new BlockAction(
                            MyTexts.TrySubstitute(tAction.Name.ToString()), null,
                            () => tAction.Apply(block.TBlock)));
                    }
                }

                if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Piston))
                {
                    members.Add(new BlockAction(
                         MyTexts.GetString(MySpaceTexts.BlockActionTitle_Reverse), null,
                         block.Piston.Reverse));
                }
                else if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Rotor))
                {
                    members.Add(new BlockAction(
                        MyTexts.GetString(MySpaceTexts.BlockActionTitle_Reverse), null,
                        block.Rotor.Reverse));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyDoor.
            /// </summary>
            public static void GetDoorActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    MyTexts.TrySubstitute("Open/Close"), null,
                    blockData.Door.ToggleDoor));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Warhead_StartCountdown),
                    () => $"({Math.Truncate(blockData.Warhead.CountdownTime)}s)",
                    blockData.Warhead.StartCountdown));
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Warhead_StopCountdown), null,
                    blockData.Warhead.StopCountdown));
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Warhead_Detonate), null,
                    blockData.Warhead.Detonate));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyLandingGear.
            /// </summary>
            public static void GetGearActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockActionTitle_SwitchLock),
                    () => $"({blockData.LandingGear.GetLocalizedStatus()})",
                    blockData.LandingGear.ToggleLock));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetConnectorActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockActionTitle_SwitchLock),
                    () => $"({blockData.Connector.GetLocalizedStatus()})",
                    blockData.Connector.ToggleConnect));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetProgrammableBlockActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_RunCode), null,
                    blockData.Program.Run));
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Recompile), null,
                    blockData.Program.Recompile));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetTimerActions(SuperBlock blockData, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_TimerStart), null,
                    blockData.Timer.StartCountdown));
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_TimerStop), null,
                    blockData.Timer.StopCountdown));
                members.Add(new BlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_TimerTrigger), null,
                    blockData.Timer.Trigger));
            }
        }
    }
}