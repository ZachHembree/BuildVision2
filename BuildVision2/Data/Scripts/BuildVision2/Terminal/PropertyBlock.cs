using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Utils;
using VRageMath;
using ChargeMode = Sandbox.ModAPI.Ingame.ChargeMode;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;
using DoorStatus = Sandbox.ModAPI.Ingame.DoorStatus;
using IMyBatteryBlock = Sandbox.ModAPI.Ingame.IMyBatteryBlock;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;
using IMyParachute = SpaceEngineers.Game.ModAPI.Ingame.IMyParachute;
using IMyTerminalAction = Sandbox.ModAPI.Interfaces.ITerminalAction;
using DarkHelmet.UI;

namespace DarkHelmet.BuildVision2
{
    internal interface IBlockMember
    {
        /// <summary>
        /// Retrieves the current value of the block member as a string
        /// </summary>
        string Value { get; }
    }

    internal interface IBlockProperty : IBlockMember
    {
        /// <summary>
        /// Retrieves the name of the block property
        /// </summary>
        string Name { get; }

        void OnSelect();

        void OnDeselect();

        /// <summary>
        /// Updates input of the property.
        /// </summary>
        void HandleInput();
    }

    internal interface IBlockAction : IBlockMember
    {
        /// <summary>
        /// Triggers action associated with block;
        /// </summary>
        void Action();
    }

    /// <summary>
    /// Encapsulates all block property data needed for the UI.
    /// </summary>
    internal class PropertyBlock
    {
        public IMyTerminalBlock TBlock { get; private set; }
        public List<IBlockProperty> Properties { get; private set; }
        public List<IBlockAction> Actions { get; private set; }
        public int Count { get { return Properties.Count + Actions.Count; } }
        public bool IsFunctional { get { return TBlock.IsFunctional; } }
        public bool IsWorking { get { return TBlock.IsWorking; } }
        public bool CanLocalPlayerAccess { get { return TBlock.HasLocalPlayerAccess(); } }
        public static PropBlockConfig Cfg { get { return BvConfig.Current.block; } set { BvConfig.Current.block = value; } }

        public PropertyBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;
            Properties = new List<IBlockProperty>(10);
            Actions = new List<IBlockAction>();

            GetScrollableProps();
            GetScrollableActions();
        }

        /// <summary>
        /// Gets the block's current position.
        /// </summary>
        public Vector3D GetPosition() =>
            TBlock.GetPosition();

        /// <summary>
        /// Retrieves all block ITerminalProperty values.
        /// </summary>
        private void GetScrollableProps()
        {
            List<ITerminalProperty> properties = new List<ITerminalProperty>(12);
            IMyTerminalControl terminalControl;
            string name;

            Properties.Add
            (
                new TextProperty("Name",
                    () => TBlock.CustomName,
                    x => TBlock.CustomName = x)
            );

            if (TBlock is IMyBatteryBlock)
            {
                IMyBatteryBlock batteryBlock = (IMyBatteryBlock)TBlock;

                Properties.Add
                (
                    new EnumProperty<ChargeMode>("Charge Mode",
                        () => batteryBlock.ChargeMode,
                        x => batteryBlock.ChargeMode = x)
                );
            }

            TBlock.GetProperties(properties);

            foreach (ITerminalProperty prop in properties)
            {
                name = GetTooltipName(prop);
                terminalControl = prop as IMyTerminalControl;

                if (name.Length > 0 && terminalControl != null && terminalControl.Visible(TBlock))
                {
                    if (prop is ITerminalProperty<bool>)
                    {
                        Properties.Add(new BoolProperty(name, (ITerminalProperty<bool>)prop, this));
                    }
                    else if (prop is ITerminalProperty<float>)
                    {
                        Properties.Add(new FloatProperty(name, (ITerminalProperty<float>)prop, this));
                    }
                    else if (prop is ITerminalProperty<Color>)
                    {
                        Properties.AddRange(ColorProperty.GetColorProperties(name, this, (ITerminalProperty<Color>)prop));
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a set of custom block actions.
        /// </summary>
        private void GetScrollableActions()
        {
            if (TBlock is IMyMechanicalConnectionBlock)
            {
                BlockAction.GetMechActions(TBlock, Actions);
            }
            else if (TBlock is IMyDoor)
            {
                BlockAction.GetDoorActions(TBlock, Actions);
            }
            else if (TBlock is IMyWarhead)
            {
                BlockAction.GetWarheadActions(TBlock, Actions);
            }
            else if (TBlock is IMyLandingGear)
            {
                BlockAction.GetGearActions(TBlock, Actions);
            }
            else if (TBlock is IMyShipConnector)
            {
                BlockAction.GetConnectorActions(TBlock, Actions);
            }
            else if (TBlock is IMyParachute)
            {
                BlockAction.GetChuteActions(TBlock, Actions);
            }
        }

        /// <summary>
        /// Retrieves a Block Property's Terminal Name
        /// </summary>
        private static string GetTooltipName(ITerminalProperty prop)
        {
            IMyTerminalControlTitleTooltip tooltip = (prop as IMyTerminalControlTitleTooltip);
            MyStringId id = tooltip == null ? MyStringId.GetOrCompute("???") : tooltip.Title;

            return MyTexts.Get(id).ToString();
        }

        /// <summary>
        /// Custom block actions
        /// </summary>
        private class BlockAction : IBlockAction
        {
            //public string Name { get; } = null;
            public string Value { get { return GetDisplay(); } }
            private Action ActionDelegate { get; set; }
            private Func<string> GetDisplay { get; set; }

            public BlockAction(Func<string> GetDisplay, Action Action)
            {
                this.GetDisplay = GetDisplay;
                this.ActionDelegate = Action;
            }

            public void Action() =>
                ActionDelegate();

            /// <summary>
            /// Gets actions for blocks implementing IMyMechanicalConnectionBlock.
            /// </summary>
            public static void GetMechActions(IMyTerminalBlock tBlock, List<IBlockAction> actions)
            {
                List<IMyTerminalAction> terminalActions = new List<IMyTerminalAction>();
                IMyMechanicalConnectionBlock mechBlock = (IMyMechanicalConnectionBlock)tBlock;
                IMyPistonBase piston;
                IMyMotorStator rotor;
                mechBlock.GetActions(terminalActions);

                foreach (IMyTerminalAction tAction in terminalActions)
                {
                    string tActionName = tAction.Name.ToString();

                    if (tAction.Name.ToString().StartsWith("Add "))
                        actions.Add(new BlockAction(
                            () => tActionName,
                            () => tAction.Apply(mechBlock)));
                }

                if (mechBlock is IMyMotorSuspension)
                {
                    actions.Add(new BlockAction(
                        () => mechBlock.IsAttached ? "Attach Wheel (Attached)" : "Attach Wheel",
                        () => mechBlock.Attach()));
                    actions.Add(new BlockAction(
                        () => "Detach Wheel",
                        () => mechBlock.Detach()));
                }
                else
                {
                    actions.Add(new BlockAction(
                        () => mechBlock.IsAttached ? "Attach Head (Attached)" : "Attach Head",
                        () => mechBlock.Attach()));
                    actions.Add(new BlockAction(
                        () => "Detach Head",
                        () => mechBlock.Detach()));
                }

                piston = mechBlock as IMyPistonBase;

                if (piston != null)
                {
                    actions.Add(new BlockAction(
                        () => "Reverse",
                        () => piston.Reverse()));
                }
                else
                {
                    rotor = mechBlock as IMyMotorStator;

                    if (rotor != null)
                        actions.Add(new BlockAction(
                            () => "Reverse",
                            () => rotor.TargetVelocityRad = -rotor.TargetVelocityRad));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyDoor.
            /// </summary>
            public static void GetDoorActions(IMyTerminalBlock tBlock, List<IBlockAction> actions)
            {
                IMyDoor doorBlock = (IMyDoor)tBlock;

                actions.Add(new BlockAction(
                    () => "Open/Close",
                    () => doorBlock.ToggleDoor()));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(IMyTerminalBlock tBlock, List<IBlockAction> actions)
            {
                IMyWarhead warhead = (IMyWarhead)tBlock;

                actions.Add(new BlockAction(
                    () => $"Start Countdown",
                    () => warhead.StartCountdown()));
                actions.Add(new BlockAction(
                    () => "Stop Countdown",
                    () => warhead.StopCountdown()));
                actions.Add(new BlockAction(
                    () => "Detonate",
                    () => warhead.Detonate()));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyLandingGear.
            /// </summary>
            public static void GetGearActions(IMyTerminalBlock tBlock, List<IBlockAction> actions)
            {
                IMyLandingGear landingGear = (IMyLandingGear)tBlock;

                actions.Add(new BlockAction(
                    () =>
                    {
                        string status = "";

                        if (landingGear.LockMode == LandingGearMode.Locked)
                            status = "Locked";
                        else if (landingGear.LockMode == LandingGearMode.ReadyToLock)
                            status = "Ready";
                        else if (landingGear.LockMode == LandingGearMode.Unlocked)
                            status = "Unlocked";

                        return $"Lock/Unlock ({status})";
                    },
                    () => landingGear.ToggleLock()));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyShipConnector.
            /// </summary>
            public static void GetConnectorActions(IMyTerminalBlock tBlock, List<IBlockAction> actions)
            {
                IMyShipConnector connector = (IMyShipConnector)tBlock;

                actions.Add(new BlockAction(
                    () =>
                    {
                        string status = "";

                        if (connector.Status == ConnectorStatus.Connected)
                            status = "Locked";
                        else if (connector.Status == ConnectorStatus.Connectable)
                            status = "Ready";
                        else if (connector.Status == ConnectorStatus.Unconnected)
                            status = "Unlocked";

                        return $"Lock/Unlock ({status})";
                    },
                    () => connector.ToggleConnect()));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyParachute.
            /// </summary>
            public static void GetChuteActions(IMyTerminalBlock tBlock, List<IBlockAction> actions)
            {
                IMyParachute parachute = (IMyParachute)tBlock;

                actions.Add(new BlockAction(
                    () =>
                    {
                        string status = "";

                        if (parachute.Status == DoorStatus.Open)
                            status = "Open";
                        else if (parachute.Status == DoorStatus.Opening)
                            status = "Opening";
                        else if (parachute.Status == DoorStatus.Closing)
                            status = "Closing";
                        else if (parachute.Status == DoorStatus.Closed)
                            status = "Closed";

                        return $"Open/Close ({status})";
                    },
                    () => parachute.ToggleDoor()));
            }
        }

        /// <summary>
        /// Field for changing block property text. 
        /// </summary>
        private class TextProperty : IBlockProperty
        {
            public string Name { get; }
            public string Value { get { return GetCurrentValue(); } }

            private bool selected;
            private readonly Func<string> GetValueFunc;
            private readonly Action<string> SetValueAction;

            public TextProperty(string name, Func<string> GetValueFunc, Action<string> SetValueAction)
            {
                Name = name;
                this.GetValueFunc = GetValueFunc;
                this.SetValueAction = SetValueAction;
            }

            public void OnSelect()
            {
                HudUtilities.TextInput.CurrentText = Value;
                selected = true;
            }

            public void OnDeselect()
            {
                HudUtilities.TextInput.Open = false;
                SetValueAction(HudUtilities.TextInput.CurrentText);
                selected = false;
            }

            public void HandleInput()
            {
                HudUtilities.TextInput.Open = selected && MyAPIGateway.Gui.ChatEntryVisible;
            }

            private string GetCurrentValue()
            {
                if (selected)
                {
                    if (MyAPIGateway.Gui.ChatEntryVisible)
                        return HudUtilities.TextInput.CurrentText;
                    else
                        return "Please Open Chat to Continue";
                }
                else
                    return GetValueFunc();
            }
        }

        /// <summary>
        /// Base for block properties that use scrolling for input.
        /// </summary>
        private abstract class ScrollablePropBase : IBlockProperty
        {
            public virtual string Name { get; protected set; }

            public abstract string Value { get; }

            public virtual void OnSelect() { }

            public virtual void OnDeselect() { }

            public void HandleInput()
            {
                if (KeyBinds.ScrollUp.IsPressedAndHeld)
                    ScrollUp();

                if (KeyBinds.ScrollDown.IsPressedAndHeld)
                    ScrollDown();
            }

            protected abstract void ScrollUp();

            protected abstract void ScrollDown();
        }

        /// <summary>
        /// Scrollable property for battery charge modes
        /// </summary>
        private class EnumProperty<T> : ScrollablePropBase where T : struct, IComparable
        {
            public override string Value { get { return GetEnumFunc().ToString(); } }

            private static readonly T[] enums;
            private readonly Func<T> GetEnumFunc;
            private readonly Action<T> SetEnumAction;
            private int index;

            static EnumProperty()
            {
                enums = (T[])Enum.GetValues(typeof(T));
            }

            public EnumProperty(string name, Func<T> GetEnumFunc, Action<T> SetEnumAction)
            {
                Name = name;
                this.GetEnumFunc = GetEnumFunc;
                this.SetEnumAction = SetEnumAction;

                index = GetEnumIndex();
            }

            protected override void ScrollDown() =>
                SetEnum(-1);

            protected override void ScrollUp() =>
                SetEnum(1);

            private void SetEnum(int scrollDir)
            {
                index += scrollDir;
                index = Utils.Math.Clamp(index, 0, enums.Length - 1);
                SetEnumAction(enums[index]);
            }

            private int GetEnumIndex()
            {
                for (int n = 0; n < enums.Length; n++)
                {
                    if (enums[n].Equals(GetEnumFunc()))
                        return n;
                }

                return -1;
            }
        }

        /// <summary>
        /// Block Terminal Property of a Boolean
        /// </summary>
        private class BoolProperty : ScrollablePropBase
        {
            public override string Value { get { return GetPropStateText(); } }

            private readonly PropertyBlock pBlock;
            private readonly ITerminalProperty<bool> prop;
            private readonly MyStringId OnText, OffText;

            public BoolProperty(string name, ITerminalProperty<bool> prop, PropertyBlock block)
            {
                this.prop = prop;
                this.pBlock = block;

                if (prop is IMyTerminalControlOnOffSwitch)
                {
                    IMyTerminalControlOnOffSwitch onOffSwitch = (IMyTerminalControlOnOffSwitch)prop;

                    OnText = onOffSwitch.OnText;
                    OffText = onOffSwitch.OffText;
                }
                else
                {
                    OnText = MySpaceTexts.SwitchText_On;
                    OffText = MySpaceTexts.SwitchText_Off;
                }

                Name = name;
            }

            protected override void ScrollDown()
            {
                if (prop.GetValue(pBlock.TBlock) == true)
                    prop.SetValue(pBlock.TBlock, false);
            }

            protected override void ScrollUp()
            {
                if (prop.GetValue(pBlock.TBlock) == false)
                    prop.SetValue(pBlock.TBlock, true);
            }

            /// <summary>
            /// Retrieves the on/off state of given property of a given block as a string.
            /// </summary>
            private string GetPropStateText()
            {
                if (prop.GetValue(pBlock.TBlock))
                    return MyTexts.Get(OnText).ToString();
                else
                    return MyTexts.Get(OffText).ToString();
            }
        }

        /// <summary>
        /// Block Terminal Property of a Float
        /// </summary>
        private class FloatProperty : ScrollablePropBase
        {
            public override string Value { get { return prop.GetValue(pBlock.TBlock).ToString(); } }

            private readonly PropertyBlock pBlock;
            private readonly ITerminalProperty<float> prop;
            private float minValue, maxValue, incrA, incrB, incrC, incr0;

            public FloatProperty(string name, ITerminalProperty<float> prop, PropertyBlock block)
            {
                this.prop = prop;
                this.pBlock = block;

                Name = name;
                minValue = this.prop.GetMinimum(pBlock.TBlock);
                maxValue = this.prop.GetMaximum(pBlock.TBlock);

                if (Name == "Pitch" || Name == "Yaw" || Name == "Roll")
                {
                    incr0 = 90f;
                }
                else
                {
                    if (float.IsInfinity(minValue) || float.IsInfinity(maxValue))
                        incr0 = 1f;
                    else
                    {
                        double range = Math.Abs(maxValue - minValue), exp;

                        if (range > maxValue)
                            exp = Math.Truncate(Math.Log10(range));
                        else
                            exp = Math.Truncate(Math.Log10(2 * range));

                        incr0 = (float)(Math.Pow(10d, exp) / Cfg.floatDiv);
                    }

                    if (incr0 == 0)
                        incr0 = 1f;
                }

                incrC = incr0 * Cfg.floatMult.Z; // x10
                incrB = incr0 * Cfg.floatMult.Y; // x5
                incrA = incr0 * Cfg.floatMult.X; // x0.1
            }

            protected override void ScrollDown() =>
                ChangePropValue(-GetIncrement());

            protected override void ScrollUp() =>
                ChangePropValue(+GetIncrement());

            /// <summary>
            /// Changes property float value based on given delta.
            /// </summary>
            private void ChangePropValue(float delta)
            {
                float current = prop.GetValue(pBlock.TBlock);

                if (float.IsInfinity(current))
                    current = 0f;

                prop.SetValue(pBlock.TBlock, (float)Math.Round(Utils.Math.Clamp((current + delta), minValue, maxValue), 3));
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private float GetIncrement()
            {
                bool
                    multA = KeyBinds.MultX.IsPressed,
                    multB = KeyBinds.MultY.IsPressed,
                    multC = KeyBinds.MultZ.IsPressed;

                if (multC)
                    return incrC; // x64
                else if (multB)
                    return incrB; // x16
                else if (multA)
                    return incrA; // x8
                else
                    return incr0; // x1
            }
        }

        /// <summary>
        /// Block Terminal Property for individual color channels of a VRageMath.Color
        /// </summary>
        private class ColorProperty : ScrollablePropBase
        {
            public override string Value { get { return colorDisp(); } }

            private readonly PropertyBlock pBlock;
            private readonly ITerminalProperty<Color> property;
            private readonly Color delta;
            private readonly Func<string> colorDisp;
            private static int minValue, maxValue, incrA, incrB, incrC, incr0;

            static ColorProperty()
            {
                minValue = byte.MinValue;
                maxValue = byte.MaxValue;
            }

            public ColorProperty(string name, PropertyBlock pBlock, ITerminalProperty<Color> prop, Color delta, Func<string> colorDisp)
            {
                this.pBlock = pBlock;
                property = prop;

                incr0 = 1;
                incrC = incr0 * Cfg.colorMult.Z; // x64
                incrB = incr0 * Cfg.colorMult.Y; // x16
                incrA = incr0 * Cfg.colorMult.X; // x8

                this.delta = delta;
                this.colorDisp = colorDisp;

                Name = name;
            }

            /// <summary>
            /// Returns a scrollable property for each color channel in an ITerminalProperty<Color> object
            /// </summary>
            public static ColorProperty[] GetColorProperties(string name, PropertyBlock pBlock, ITerminalProperty<Color> prop)
            {
                return new ColorProperty[]
                {
                    new ColorProperty(name, pBlock, prop, new Color(1, 0, 0), () => (" R: " + prop.GetValue(pBlock.TBlock).R)), // R
                    new ColorProperty(name, pBlock, prop, new Color(0, 1, 0), () => (" G: " + prop.GetValue(pBlock.TBlock).G)), // G
                    new ColorProperty(name, pBlock, prop, new Color(0, 0, 1), () => (" B: " + prop.GetValue(pBlock.TBlock).B))  // B
                };
            }

            protected override void ScrollDown() =>
                ChangePropValue(false);

            protected override void ScrollUp() =>
                ChangePropValue(true);

            /// <summary>
            /// Changes property color value based on given color delta.
            /// </summary>
            private void ChangePropValue(bool increment)
            {
                Color curr = property.GetValue(pBlock.TBlock);
                int r = curr.R, g = curr.G, b = curr.B,
                    sign = increment ? 1 : -1, mult = GetIncrement();

                r += (sign * mult * delta.R);
                g += (sign * mult * delta.G);
                b += (sign * mult * delta.B);

                curr.R = (byte)Utils.Math.Clamp(r, minValue, maxValue);
                curr.G = (byte)Utils.Math.Clamp(g, minValue, maxValue);
                curr.B = (byte)Utils.Math.Clamp(b, minValue, maxValue);

                property.SetValue(pBlock.TBlock, curr);
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private int GetIncrement()
            {
                bool
                    multA = KeyBinds.MultX.IsPressed,
                    multB = KeyBinds.MultY.IsPressed,
                    multC = KeyBinds.MultZ.IsPressed;

                if (multC)
                    return incrC; // x64
                else if (multB)
                    return incrB; // x16
                else if (multA)
                    return incrA; // x8
                else
                    return incr0; // x1
            }
        }
    }
}