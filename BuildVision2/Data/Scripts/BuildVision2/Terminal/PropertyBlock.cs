using DarkHelmet.UI;
using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;
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

        /// <summary>
        /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
        /// </summary>
        bool Enabled { get; }
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
        public int EnabledElementCount { get { return GetEnabledElementCount(); } }
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

        private int GetEnabledElementCount()
        {
            int count = 0;

            foreach (IBlockAction action in Actions)
                if (action.Enabled)
                    count++;

            foreach (IBlockProperty property in Properties)
                if (property.Enabled)
                    count++;

            return count;
        }

        /// <summary>
        /// Retrieves a Block Property's Terminal Name. Filters out any characters that aren't numbers, letters or spaces.
        /// </summary>
        private static string GetTooltipName(ITerminalProperty prop)
        {
            if (prop is IMyTerminalControlTitleTooltip tooltip)
            {
                int trailingSpaceLength = 0;
                StringBuilder name = MyTexts.Get(tooltip.Title),
                    cleanedName = new StringBuilder(name.Length);

                for (int n = name.Length - 1; (n >= 0 && name[n] == ' '); n--)
                    trailingSpaceLength++;

                for (int n = 0; n < name.Length - trailingSpaceLength; n++)
                {
                    if ((name[n] >= '0' && name[n] <= '9') || (name[n] >= 'A' && name[n] <= 'z') || name[n] == ' ')
                        cleanedName.Append(name[n]);
                }

                return cleanedName.ToString();
            }
            else
                return "";
        }

        /// <summary>
        /// Filters out any any special characters from a given string.
        /// </summary>
        private static string CleanText(StringBuilder text)
        {
            StringBuilder cleanedText = new StringBuilder(text.Length);

            for (int n = 0; n < text.Length; n++)
            {
                if (text[n] >= ' ' && text[n] <= '~')
                    cleanedText.Append(text[n]);
            }

            return cleanedText.ToString();
        }

        /// <summary>
        /// Retrieves all block ITerminalProperty values.
        /// </summary>
        private void GetScrollableProps()
        {
            List<ITerminalProperty> properties = new List<ITerminalProperty>(12);
            string name;

            TBlock.GetProperties(properties);
            Properties.Add(null);

            foreach (ITerminalProperty prop in properties)
            {
                if (prop is IMyTerminalControl control)
                {
                    name = GetTooltipName(prop);

                    if (name.Length > 0)
                    {
                        if (prop is ITerminalProperty<StringBuilder> textProp)
                        {
                            if (name == "Name")
                                Properties[0] = new TextProperty(name, textProp, control, TBlock);
                            else
                                Properties.Add(new TextProperty(name, textProp, control, TBlock));
                        }
                        if (prop is IMyTerminalControlCombobox comboBox && !name.StartsWith("Assign"))
                        {
                            Properties.Add(new ComboBoxProperty(name, comboBox, control, TBlock));
                        }
                        else if (prop is ITerminalProperty<bool> boolProp)
                        {
                            Properties.Add(new BoolProperty(name, boolProp, control, TBlock));
                        }
                        else if (prop is ITerminalProperty<float> floatProp)
                        {
                            Properties.Add(new FloatProperty(name, floatProp, control, TBlock));
                        }
                        else if (prop is ITerminalProperty<Color> colorProp)
                        {
                            Properties.AddRange(ColorProperty.GetColorProperties(name, colorProp, control, TBlock));
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
            public bool Enabled { get; }
            private Action ActionDelegate { get; set; }
            private Func<string> GetDisplayFunc { get; set; }

            public BlockAction(Func<string> GetDisplayFunc, Action Action)
            {
                Enabled = true;
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
        /// Base class for all Build Vision terminal properties that make use of SE's <see cref="ITerminalProperty"/>
        /// </summary>
        private abstract class BvTerminalProperty<T> : IBlockProperty where T : ITerminalProperty
        {
            public virtual string Name { get; protected set; }
            public abstract string Value { get; }
            public virtual bool Enabled { get { return control.Enabled(block) && control.Visible(block); } }

            protected readonly T property;
            protected readonly IMyTerminalControl control;
            protected readonly IMyTerminalBlock block;

            protected BvTerminalProperty(string name, T property, IMyTerminalControl control, IMyTerminalBlock block)
            {
                Name = name;
                this.property = property;
                this.control = control;
                this.block = block;
            }

            public virtual void HandleInput() { }

            public virtual void OnSelect() { }

            public virtual void OnDeselect() { }
        }

        /// <summary>
        /// Field for changing block property text. 
        /// </summary>
        private class TextProperty : BvTerminalProperty<ITerminalProperty<StringBuilder>>
        {
            public override string Value { get { return GetCurrentValue(); } }

            private const long blinkInterval = TimeSpan.TicksPerMillisecond * 500;
            private bool selected, blink;
            private long lastTime;

            public TextProperty(string name, ITerminalProperty<StringBuilder> textProp, IMyTerminalControl control, IMyTerminalBlock block) : base(name, textProp, control, block)
            {
                lastTime = long.MinValue;
            }

            public override void OnSelect()
            {
                HudUtilities.TextInput.CurrentText = CleanText(property.GetValue(block));
                lastTime = DateTime.Now.Ticks;
                selected = true;
            }

            public override void OnDeselect()
            {
                HudUtilities.TextInput.Open = false;
                property.SetValue(block, new StringBuilder(HudUtilities.TextInput.CurrentText));
                selected = false;
            }

            public override void HandleInput()
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
                    return CleanText(property.GetValue(block));
            }
        }

        /// <summary>
        /// Base for block properties that use scrolling for input.
        /// </summary>
        private abstract class ScrollablePropBase<T> : BvTerminalProperty<T> where T : ITerminalProperty
        {
            protected ScrollablePropBase(string name, T property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            { }

            public override void HandleInput()
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
        /// Scrollable property for <see cref="IMyTerminalControlCombobox"/> terminal properties.
        /// </summary>
        private class ComboBoxProperty : ScrollablePropBase<IMyTerminalControlCombobox>
        {
            public override string Value { get { return names[GetCurrentIndex()]; } }

            private readonly List<long> keys;
            private readonly List<string> names;

            public ComboBoxProperty(string name, IMyTerminalControlCombobox comboBox, IMyTerminalControl control, IMyTerminalBlock block) : base(name, comboBox, control, block)
            {
                List<MyTerminalControlComboBoxItem> content = new List<MyTerminalControlComboBoxItem>();
                comboBox.ComboBoxContent(content);

                keys = new List<long>(content.Count);
                names = new List<string>(content.Count);

                foreach (MyTerminalControlComboBoxItem item in content)
                {
                    string itemName = MyTexts.Get(item.Value).ToString();
                    keys.Add(item.Key);
                    names.Add(itemName);
                }
            }

            protected override void ScrollUp() =>
                ChangePropValue(1);

            protected override void ScrollDown() =>
                ChangePropValue(-1);

            private void ChangePropValue(int delta)
            {
                int index = Utils.Math.Clamp((GetCurrentIndex() + delta), 0, keys.Count - 1);
                property.Setter(block, keys[index]);
            }

            private int GetCurrentIndex()
            {
                long key = property.Getter(block);

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
        private class BoolProperty : ScrollablePropBase<ITerminalProperty<bool>>
        {
            public override string Value { get { return GetPropStateText(); } }

            private readonly MyStringId OnText, OffText;

            public BoolProperty(string name, ITerminalProperty<bool> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                if (property is IMyTerminalControlOnOffSwitch onOffSwitch)
                {
                    OnText = onOffSwitch.OnText;
                    OffText = onOffSwitch.OffText;
                }
                else
                {
                    OnText = MySpaceTexts.SwitchText_On;
                    OffText = MySpaceTexts.SwitchText_Off;
                }
            }

            protected override void ScrollDown()
            {
                if (property.GetValue(block) == true)
                    property.SetValue(block, false);
            }

            protected override void ScrollUp()
            {
                if (property.GetValue(block) == false)
                    property.SetValue(block, true);
            }

            /// <summary>
            /// Retrieves the on/off state of given property of a given block as a string.
            /// </summary>
            private string GetPropStateText()
            {
                if (property.GetValue(block))
                    return MyTexts.Get(OnText).ToString();
                else
                    return MyTexts.Get(OffText).ToString();
            }
        }

        /// <summary>
        /// Block Terminal Property of a Float
        /// </summary>
        private class FloatProperty : ScrollablePropBase<ITerminalProperty<float>>
        {
            public override string Value { get { return property.GetValue(block).ToString(); } }

            private readonly float minValue, maxValue, incrX, incrY, incrZ, incr0;

            public FloatProperty(string name, ITerminalProperty<float> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                minValue = this.property.GetMinimum(block);
                maxValue = this.property.GetMaximum(block);

                if (Name == "Pitch" || Name == "Yaw" || Name == "Roll")
                    incr0 = 90f;
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

                incrZ = incr0 * Cfg.floatMult.Z; // x10
                incrY = incr0 * Cfg.floatMult.Y; // x5
                incrX = incr0 * Cfg.floatMult.X; // x0.1
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
                float current = property.GetValue(block);

                if (float.IsInfinity(current))
                    current = 0f;

                property.SetValue(block, (float)Math.Round(Utils.Math.Clamp((current + delta), minValue, maxValue), 3));
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private float GetIncrement()
            {
                if (KeyBinds.MultZ.IsPressed)
                    return incrZ;
                else if (KeyBinds.MultY.IsPressed)
                    return incrY;
                else if (KeyBinds.MultX.IsPressed)
                    return incrX;
                else
                    return incr0;
            }
        }

        /// <summary>
        /// Block Terminal Property for individual color channels of a VRageMath.Color
        /// </summary>
        private class ColorProperty : ScrollablePropBase<ITerminalProperty<Color>>
        {
            public override string Value { get { return colorDisp(); } }

            private readonly Color delta;
            private readonly Func<string> colorDisp;
            private static int incrX, incrY, incrZ, incr0;

            public ColorProperty(string name, ITerminalProperty<Color> property, IMyTerminalControl control, IMyTerminalBlock block, Color delta, Func<string> colorDisp)
                : base(name, property, control, block)
            {
                incr0 = 1;
                incrZ = incr0 * Cfg.colorMult.Z; // x64
                incrY = incr0 * Cfg.colorMult.Y; // x16
                incrX = incr0 * Cfg.colorMult.X; // x8

                this.delta = delta;
                this.colorDisp = colorDisp;
            }

            /// <summary>
            /// Returns a scrollable property for each color channel in an ITerminalProperty<Color> object
            /// </summary>
            public static ColorProperty[] GetColorProperties(string name, ITerminalProperty<Color> property, IMyTerminalControl control, IMyTerminalBlock block)
            {
                return new ColorProperty[]
                {
                    new ColorProperty(name, property, control, block, new Color(1, 0, 0), () => (" R: " + property.GetValue(block).R)), // R
                    new ColorProperty(name, property, control, block, new Color(0, 1, 0), () => (" G: " + property.GetValue(block).G)), // G
                    new ColorProperty(name, property, control, block, new Color(0, 0, 1), () => (" B: " + property.GetValue(block).B))  // B
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
                Color current = property.GetValue(block);
                int r = current.R, g = current.G, b = current.B,
                    mult = increment ? GetIncrement() : -GetIncrement();

                r += (mult * delta.R);
                g += (mult * delta.G);
                b += (mult * delta.B);

                current.R = (byte)Utils.Math.Clamp(r, byte.MinValue, byte.MaxValue);
                current.G = (byte)Utils.Math.Clamp(g, byte.MinValue, byte.MaxValue);
                current.B = (byte)Utils.Math.Clamp(b, byte.MinValue, byte.MaxValue);

                property.SetValue(block, current);
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private int GetIncrement()
            {
                if (KeyBinds.MultZ.IsPressed)
                    return incrZ;
                else if (KeyBinds.MultY.IsPressed)
                    return incrY;
                else if (KeyBinds.MultX.IsPressed)
                    return incrX;
                else
                    return incr0;
            }
        }
    }
}