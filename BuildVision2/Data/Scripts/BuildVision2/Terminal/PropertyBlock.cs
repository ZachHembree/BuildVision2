using DarkHelmet.UI;
using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Text;
using System.Collections.Generic;
using VRage;
using VRage.Utils;
using VRage.ModAPI;
using VRageMath;
using ChargeMode = Sandbox.ModAPI.Ingame.ChargeMode;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;
using IMyBatteryBlock = Sandbox.ModAPI.Ingame.IMyBatteryBlock;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;
using IMyParachute = SpaceEngineers.Game.ModAPI.Ingame.IMyParachute;
using IMyTerminalAction = Sandbox.ModAPI.Interfaces.ITerminalAction;

namespace DarkHelmet.BuildVision2
{
    internal interface IBlockMember
    {
        /// <summary>
        /// Retrieves the current value of the block member as a <see cref="string"/>
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
        public int ElementCount { get { return Properties.Count + Actions.Count; } }
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
        /// Retrieves a Block Property's Terminal Name
        /// </summary>
        private static string GetTooltipName(ITerminalProperty prop)
        {
            if (prop is IMyTerminalControlTitleTooltip tooltip)
                return MyTexts.Get(tooltip.Title).ToString();
            else
                return "";
        }

        /// <summary>
        /// Retrieves all block ITerminalProperty values.
        /// </summary>
        private void GetScrollableProps()
        {
            List<ITerminalProperty> properties = new List<ITerminalProperty>(12);
            string name;

            TBlock.GetProperties(properties);

            foreach (ITerminalProperty prop in properties)
            {
                if (prop is IMyTerminalControl terminalControl && terminalControl.Visible(TBlock) && terminalControl.Enabled(TBlock))
                {
                    name = GetTooltipName(prop);

                    if (name.Length > 0)
                    {
                        if (prop is ITerminalProperty<StringBuilder> textProp)
                        {
                            if (name == "Name")
                                Properties.Insert(0, new TextProperty(name, textProp, TBlock));
                            else
                                Properties.Add(new TextProperty(name, textProp, TBlock));
                        }
                        if (prop is IMyTerminalControlCombobox comboBox && !name.StartsWith("Assigned"))
                        {
                            Properties.Add(new ComboBoxProperty(name, comboBox, TBlock));
                        }
                        else if (prop is ITerminalProperty<bool> boolProp)
                        {
                            Properties.Add(new BoolProperty(name, boolProp, TBlock));
                        }
                        else if (prop is ITerminalProperty<float> floatProp)
                        {
                            Properties.Add(new FloatProperty(name, floatProp, TBlock));
                        }
                        else if (prop is ITerminalProperty<Color> colorProp)
                        {
                            Properties.AddRange(ColorProperty.GetColorProperties(name, TBlock, colorProp));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a set of custom block actions.
        /// </summary>
        private void GetScrollableActions()
        {
            if (TBlock is IMyMechanicalConnectionBlock mechBlock)
            {
                BlockAction.GetMechActions(mechBlock, Actions);
            }
            else if (TBlock is IMyDoor door)
            {
                BlockAction.GetDoorActions(door, Actions);
            }
            else if (TBlock is IMyWarhead warhead)
            {
                BlockAction.GetWarheadActions(warhead, Actions);
            }
            else if (TBlock is IMyLandingGear landingGear)
            {
                BlockAction.GetGearActions(landingGear, Actions);
            }
            else if (TBlock is IMyShipConnector connector)
            {
                BlockAction.GetConnectorActions(connector, Actions);
            }
            else if (TBlock is IMyParachute chute)
            {
                BlockAction.GetChuteActions(chute, Actions);
            }
        }

        /// <summary>
        /// Custom block actions
        /// </summary>
        private class BlockAction : IBlockAction
        {
            public string Value { get { return GetDisplayFunc(); } }
            private Action ActionDelegate { get; set; }
            private Func<string> GetDisplayFunc { get; set; }

            public BlockAction(Func<string> GetDisplayFunc, Action Action)
            {
                this.GetDisplayFunc = GetDisplayFunc;
                this.ActionDelegate = Action;
            }

            public void Action() =>
                ActionDelegate();

            /// <summary>
            /// Gets actions for blocks implementing IMyMechanicalConnectionBlock.
            /// </summary>
            public static void GetMechActions(IMyMechanicalConnectionBlock mechBlock, List<IBlockAction> actions)
            {
                List<IMyTerminalAction> terminalActions = new List<IMyTerminalAction>();
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

                if (mechBlock is IMyPistonBase piston)
                {
                    actions.Add(new BlockAction(
                        () => "Reverse",
                        () => piston.Reverse()));
                }
                else if (mechBlock is IMyMotorStator rotor)
                {
                    actions.Add(new BlockAction(
                            () => "Reverse",
                            () => rotor.TargetVelocityRad = -rotor.TargetVelocityRad));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyDoor.
            /// </summary>
            public static void GetDoorActions(IMyDoor doorBlock, List<IBlockAction> actions)
            {
                actions.Add(new BlockAction(
                    () => "Open/Close",
                    () => doorBlock.ToggleDoor()));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(IMyWarhead warhead, List<IBlockAction> actions)
            {
                actions.Add(new BlockAction(
                    () => "Start Countdown",
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
            public static void GetGearActions(IMyLandingGear landingGear, List<IBlockAction> actions)
            {
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
            public static void GetConnectorActions(IMyShipConnector connector, List<IBlockAction> actions)
            {
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
            public static void GetChuteActions(IMyParachute parachute, List<IBlockAction> actions)
            {
                actions.Add(new BlockAction(
                    () => $"Open/Close ({parachute.Status.ToString()})",
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

            private bool selected, blink;
            private long lastTime;
            private readonly StringBuilder text;
            private const long blinkInterval = TimeSpan.TicksPerMillisecond * 500;

            public TextProperty(string name, ITerminalProperty<StringBuilder> textProp, IMyTerminalBlock block)
            {
                Name = name;
                text = textProp.GetValue(block);
                lastTime = long.MinValue;

                StringBuilder newText = new StringBuilder(text.Length);

                for (int n = 0; n < text.Length; n++)
                {
                    if (text[n] != '\t')
                        newText.Append(text[n]);
                }

                text = newText;
            }

            public void OnSelect()
            {
                HudUtilities.TextInput.CurrentText = text.ToString();
                lastTime = DateTime.Now.Ticks;
                selected = true;
            }

            public void OnDeselect()
            {
                HudUtilities.TextInput.Open = false;
                text.Clear();
                text.Append(HudUtilities.TextInput.CurrentText);
                selected = false;
            }

            public void HandleInput()
            {
                HudUtilities.TextInput.Open = selected && MyAPIGateway.Gui.ChatEntryVisible;

                if (DateTime.Now.Ticks >= lastTime + blinkInterval)
                {
                    blink = !blink;
                    lastTime += blinkInterval;

                    if (DateTime.Now.Ticks > lastTime)
                        lastTime = DateTime.Now.Ticks;
                }
            }

            private string GetCurrentValue()
            {
                if (selected)
                {
                    if (MyAPIGateway.Gui.ChatEntryVisible)
                    {
                        if (blink)
                            return HudUtilities.TextInput.CurrentText + '|';
                        else
                            return HudUtilities.TextInput.CurrentText;
                    }
                    else
                        return "Open Chat to Continue";
                }
                else
                    return text.ToString();
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
        /// Scrollable property for <see cref="IMyTerminalControlCombobox"/> / <see cref="ITerminalProperty{long}"/> terminal properties.
        /// </summary>
        private class ComboBoxProperty : ScrollablePropBase
        {
            public override string Value { get { return names?[GetCurrentIndex()]; } }

            private readonly IMyTerminalBlock block;
            private readonly List<long> keys;
            private readonly List<string> names;
            private readonly IMyTerminalControlCombobox comboBox;

            public ComboBoxProperty(string name, IMyTerminalControlCombobox comboBox, IMyTerminalBlock block)
            {
                this.comboBox = comboBox;
                this.block = block;
                Name = name;
                keys = new List<long>();
                names = new List<string>();

                List<MyTerminalControlComboBoxItem> content = new List<MyTerminalControlComboBoxItem>();
                comboBox.ComboBoxContent(content);

                foreach (MyTerminalControlComboBoxItem item in content)
                {
                    string itemName = MyTexts.Get(item.Value)?.ToString();

                    if (itemName != null)
                    {
                        keys.Add(item.Key);
                        names.Add(itemName);
                    }
                }
            }

            protected override void ScrollUp() =>
                ChangePropValue(1);

            protected override void ScrollDown() =>
                ChangePropValue(-1);

            private void ChangePropValue(int delta)
            {
                int index = Utils.Math.Clamp((GetCurrentIndex() + delta), 0, keys.Count - 1);
                comboBox.Setter(block, keys[index]);
            }

            private int GetCurrentIndex()
            {
                long key = comboBox.Getter(block);

                for (int n = 0; n < keys.Count; n++)
                {
                    if (keys[n] == key)
                        return n;
                }

                return 0;
            }
        }

        /// <summary>
        /// Block Terminal Property of a Boolean
        /// </summary>
        private class BoolProperty : ScrollablePropBase
        {
            public override string Value { get { return GetPropStateText(); } }

            private readonly IMyTerminalBlock block;
            private readonly ITerminalProperty<bool> prop;
            private readonly MyStringId OnText, OffText;

            public BoolProperty(string name, ITerminalProperty<bool> prop, IMyTerminalBlock block)
            {
                this.prop = prop;
                this.block = block;

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
                if (prop.GetValue(block) == true)
                    prop.SetValue(block, false);
            }

            protected override void ScrollUp()
            {
                if (prop.GetValue(block) == false)
                    prop.SetValue(block, true);
            }

            /// <summary>
            /// Retrieves the on/off state of given property of a given block as a string.
            /// </summary>
            private string GetPropStateText()
            {
                if (prop.GetValue(block))
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
            public override string Value { get { return prop.GetValue(block).ToString(); } }

            private readonly IMyTerminalBlock block;
            private readonly ITerminalProperty<float> prop;
            private readonly float minValue, maxValue, incrA, incrB, incrC, incr0;

            public FloatProperty(string name, ITerminalProperty<float> prop, IMyTerminalBlock block)
            {
                this.prop = prop;
                this.block = block;

                Name = name;
                minValue = this.prop.GetMinimum(block);
                maxValue = this.prop.GetMaximum(block);

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
                float current = prop.GetValue(block);

                if (float.IsInfinity(current))
                    current = 0f;

                prop.SetValue(block, (float)Math.Round(Utils.Math.Clamp((current + delta), minValue, maxValue), 3));
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

            private readonly IMyTerminalBlock block;
            private readonly ITerminalProperty<Color> property;
            private readonly Color delta;
            private readonly Func<string> colorDisp;
            private static readonly int minValue;
            private static readonly int maxValue;
            private static int incrA;
            private static int incrB;
            private static int incrC;
            private static int incr0;

            static ColorProperty()
            {
                minValue = byte.MinValue;
                maxValue = byte.MaxValue;
            }

            public ColorProperty(string name, IMyTerminalBlock block, ITerminalProperty<Color> prop, Color delta, Func<string> colorDisp)
            {
                this.block = block;
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
            public static ColorProperty[] GetColorProperties(string name, IMyTerminalBlock block, ITerminalProperty<Color> prop)
            {
                return new ColorProperty[]
                {
                    new ColorProperty(name, block, prop, new Color(1, 0, 0), () => (" R: " + prop.GetValue(block).R)), // R
                    new ColorProperty(name, block, prop, new Color(0, 1, 0), () => (" G: " + prop.GetValue(block).G)), // G
                    new ColorProperty(name, block, prop, new Color(0, 0, 1), () => (" B: " + prop.GetValue(block).B))  // B
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
                Color curr = property.GetValue(block);
                int r = curr.R, g = curr.G, b = curr.B,
                    sign = increment ? 1 : -1, mult = GetIncrement();

                r += (sign * mult * delta.R);
                g += (sign * mult * delta.G);
                b += (sign * mult * delta.B);

                curr.R = (byte)Utils.Math.Clamp(r, minValue, maxValue);
                curr.G = (byte)Utils.Math.Clamp(g, minValue, maxValue);
                curr.B = (byte)Utils.Math.Clamp(b, minValue, maxValue);

                property.SetValue(block, curr);
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