using RichHudFramework;
using RichHudFramework.Internal;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;
using IMyTerminalAction = Sandbox.ModAPI.Interfaces.ITerminalAction;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Custom block actions
        /// </summary>
        private class BlockAction : BlockMemberBase, IBlockAction
        {
            public override StringBuilder FormattedValue { get { GetValueFunc(dispBuilder); return dispBuilder; } }

            public override StringBuilder StatusText 
            { 
                get 
                {
                    if (GetPostfixFunc != null)
                    {
                        GetPostfixFunc(statusBuilder);
                        return statusBuilder;
                    }
                    else
                        return null;
                } 
            }

            private Action<StringBuilder> GetValueFunc, GetPostfixFunc;
            private Action action;
            private BvPropPool<BlockAction> poolParent;
            private readonly StringBuilder dispBuilder, statusBuilder;

            public BlockAction()
            {
                dispBuilder = new StringBuilder();
                statusBuilder = new StringBuilder();
                ValueType = BlockMemberValueTypes.None;
            }

            public void SetAction(Action<StringBuilder> GetValueFunc, Action<StringBuilder> GetPostfixFunc, Action Action, PropertyBlock block)
            {
                if (poolParent == null)
                    poolParent = block.blockActionPool;

                Name = null;
                Enabled = true;

                this.GetValueFunc = GetValueFunc;
                this.GetPostfixFunc = GetPostfixFunc;
                action = Action;
            }

            public override void Reset()
            {
                dispBuilder.Clear();
                statusBuilder.Clear();
                GetValueFunc = null;
                GetPostfixFunc = null;
                action = null;
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            public static BlockAction GetBlockAction(Action<StringBuilder> GetValueFunc, Action<StringBuilder> GetPostfixFunc, Action Action, PropertyBlock block)
            {
                BlockAction blockAction = block.blockActionPool.Get();
                blockAction.SetAction(GetValueFunc, GetPostfixFunc, Action, block);

                return blockAction;
            }

            public static BlockAction GetBlockAction(string value, Action<StringBuilder> GetPostfixFunc, Action Action, PropertyBlock block)
            {
                BlockAction blockAction = block.blockActionPool.Get();
                blockAction.SetAction(x => { if (x.Length == 0) x.Append(value); }, GetPostfixFunc, Action, block);

                return blockAction;
            }

            public void Action() =>
                action();

            /// <summary>
            /// Gets actions for blocks implementing IMyMechanicalConnectionBlock.
            /// </summary>
            public static void GetMechActions(PropertyBlock block, List<BlockMemberBase> members)
            {
                List<IMyTerminalAction> terminalActions = new List<IMyTerminalAction>();
                block.TBlock.GetActions(terminalActions);

                IMyTerminalAction hotBarAttach = terminalActions.Find(x => x.Id == "Attach");
                Action AttachAction;

                if (hotBarAttach != null)
                    AttachAction = () => hotBarAttach.Apply(block.TBlock);
                else
                    AttachAction = block.MechConnection.AttachHead;

                members.Add(GetBlockAction(
                    // Name
                    MyTexts.GetString(MySpaceTexts.BlockActionTitle_Attach),
                    // Status
                    x => 
                    {
                        x.Clear();
                        x.Append('(');
                        x.Append(block.MechConnection.GetLocalizedAttachStatus());
                        x.Append(')');
                    },
                    AttachAction, block));
                members.Add(GetBlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockActionTitle_Detach), null,
                    block.MechConnection.DetachHead, block));

                foreach (IMyTerminalAction tAction in terminalActions)
                {
                    if (tAction.IsEnabled(block.TBlock) && tAction.Id.StartsWith("Add"))
                    {
                        members.Add(GetBlockAction(
                            MyTexts.TrySubstitute(tAction.Name.ToString()), null,
                            () => tAction.Apply(block.TBlock), block));
                    }
                }

                if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Piston))
                {
                    members.Add(GetBlockAction(
                         MyTexts.GetString(MySpaceTexts.BlockActionTitle_Reverse), null,
                         block.Piston.Reverse, block));
                }
                else if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Rotor))
                {
                    members.Add(GetBlockAction(
                        MyTexts.GetString(MySpaceTexts.BlockActionTitle_Reverse), null,
                        block.Rotor.Reverse, block));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyDoor.
            /// </summary>
            public static void GetDoorActions(PropertyBlock block, List<BlockMemberBase> members)
            {
                members.Add(GetBlockAction(
                    MyTexts.TrySubstitute("Open/Close"), null,
                    block.Door.ToggleDoor, block));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(PropertyBlock block, List<BlockMemberBase> members)
            {
                members.Add(GetBlockAction(
                    // Name
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Warhead_StartCountdown),
                    // Status
                    x => 
                    {
                        x.Clear();
                        x.Append('(');
                        x.Append(Math.Truncate(block.Warhead.CountdownTime));
                        x.Append("s)");
                    },
                    block.Warhead.StartCountdown, block));
                members.Add(GetBlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Warhead_StopCountdown), null,
                    block.Warhead.StopCountdown, block));
                members.Add(GetBlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Warhead_Detonate), null,
                    block.Warhead.Detonate, block));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyLandingGear.
            /// </summary>
            public static void GetGearActions(PropertyBlock block, List<BlockMemberBase> members)
            {
                members.Add(GetBlockAction(
                    // Name
                    MyTexts.GetString(MySpaceTexts.BlockActionTitle_SwitchLock),
                    // Status
                    x => 
                    {
                        x.Clear();
                        x.Append('(');
                        x.Append(block.LandingGear.GetLocalizedStatus());
                        x.Append(')');
                    },
                    block.LandingGear.ToggleLock, block));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetConnectorActions(PropertyBlock block, List<BlockMemberBase> members)
            {
                members.Add(GetBlockAction(
                    // Name
                    MyTexts.GetString(MySpaceTexts.BlockActionTitle_SwitchLock),
                    // Status
                    x => 
                    {
                        x.Clear();
                        x.Append('(');
                        x.Append(block.Connector.GetLocalizedStatus());
                        x.Append(')');
                    },
                    block.Connector.ToggleConnect, block));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetProgrammableBlockActions(PropertyBlock block, List<BlockMemberBase> members)
            {
                members.Add(GetBlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_RunCode), null,
                    block.Program.Run, block));
                members.Add(GetBlockAction(
                    MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Recompile), null,
                    block.Program.Recompile, block));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetTimerActions(PropertyBlock block, List<BlockMemberBase> members)
            {
                members.Add(GetBlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_TimerStart), null,
                    block.Timer.StartCountdown, block));
                members.Add(GetBlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_TimerStop), null,
                    block.Timer.StopCountdown, block));
                members.Add(GetBlockAction(
                    MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_TimerTrigger), null,
                    block.Timer.Trigger, block));
            }
        }
    }
}