using DarkHelmet.UI;
using Sandbox.Game.Localization;
using Sandbox.Game.Entities;
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
    [Flags]
    internal enum BlockInputType : byte
    {
        None = 0,
        Scroll = 1,
        Text = 2
    }

    internal interface IBlockMember
    {
        /// <summary>
        /// Retrieves the name of the block property
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Retrieves the current value of the block member as a <see cref="string"/>
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
        /// </summary>
        bool Enabled { get; }

        BlockInputType InputType { get; }

        void OnSelect();

        void OnDeselect();

        /// <summary>
        /// Updates input of the property.
        /// </summary>
        void HandleInput();
    }

    /// <summary>
    /// Encapsulates all block property data needed for the UI.
    /// </summary>
    internal class PropertyBlock
    {
        public IMyTerminalBlock TBlock { get; private set; }
        public IMyBlockGroup Group { get; private set; }
        public ReadOnlyCollection<IBlockMember> BlockMembers { get; private set; }
        public int EnabledMembers => GetEnabledElementCount();
        public bool IsFunctional { get { return TBlock.IsFunctional; } }
        public bool IsWorking { get { return TBlock.IsWorking; } }
        public bool CanLocalPlayerAccess { get { return TBlock.HasLocalPlayerAccess(); } }
        public static PropBlockConfig Cfg { get { return BvConfig.Current.block; } set { BvConfig.Current.block = value; } }

        private readonly List<IBlockMember> blockMembers;

        public PropertyBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;
            blockMembers = new List<IBlockMember>();
            BlockMembers = new ReadOnlyCollection<IBlockMember>(blockMembers);

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

            foreach (IBlockMember member in blockMembers)
                if (member.Enabled)
                    count++;

            return count;
        }

        /// <summary>
        /// Retrieves a Block Property's Terminal Name.
        /// </summary>
        private static string GetTooltipName(ITerminalProperty prop)
        {
            if (prop is IMyTerminalControlTitleTooltip)
            {
                IMyTerminalControlTitleTooltip tooltip = (IMyTerminalControlTitleTooltip)prop;
                int trailingSpaceLength = 0;
                StringBuilder name = MyTexts.Get(tooltip.Title),
                    cleanedName = new StringBuilder(name.Length);

                for (int n = name.Length - 1; (n >= 0 && name[n] == ' '); n--)
                    trailingSpaceLength++;

                for (int n = 0; n < name.Length - trailingSpaceLength; n++)
                {
                    if (name[n] > 31)
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
                if (text[n] > 31)
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
            string name, nameField = MyTexts.TrySubstitute("Name");
            TBlock.GetProperties(properties);

            foreach (ITerminalProperty prop in properties)
            {
                if (prop is IMyTerminalControl)
                {
                    IMyTerminalControl control = (IMyTerminalControl)prop;
                    name = GetTooltipName(prop);

                    if (name.Length > 0)
                    {
                        if (prop is ITerminalProperty<StringBuilder>)
                        {
                            ITerminalProperty<StringBuilder> textProp = (ITerminalProperty<StringBuilder>)prop;

                            if (name == nameField)
                                blockMembers.Insert(0, new TextProperty(name, textProp, control, TBlock));
                            else
                                blockMembers.Add(new TextProperty(name, textProp, control, TBlock));
                        }
                        if (prop is IMyTerminalControlCombobox) // fields having to do with camera assignments seem to give me trouble here
                        {
                            try
                            {
                                blockMembers.Add(new ComboBoxProperty(name, (IMyTerminalControlCombobox)prop, control, TBlock));
                            }
                            catch { }
                        }
                        else if (prop is ITerminalProperty<bool>)
                        {
                            blockMembers.Add(new BoolProperty(name, (ITerminalProperty<bool>)prop, control, TBlock));
                        }
                        else if (prop is ITerminalProperty<float>)
                        {
                            blockMembers.Add(new FloatProperty(name, (ITerminalProperty<float>)prop, control, TBlock));
                        }
                        else if (prop is ITerminalProperty<Color>)
                        {
                            blockMembers.AddRange(ColorProperty.GetColorProperties(name, (ITerminalProperty<Color>)prop, control, TBlock));
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
            if (TBlock is IMyMechanicalConnectionBlock)
            {
                BlockAction.GetMechActions((IMyMechanicalConnectionBlock)TBlock, blockMembers);
            }
            else if (TBlock is IMyDoor)
            {
                BlockAction.GetDoorActions((IMyDoor)TBlock, blockMembers);
            }
            else if (TBlock is IMyWarhead)
            {
                BlockAction.GetWarheadActions((IMyWarhead)TBlock, blockMembers);
            }
            else if (TBlock is IMyLandingGear)
            {
                BlockAction.GetGearActions((IMyLandingGear)TBlock, blockMembers);
            }
            else if (TBlock is IMyShipConnector)
            {
                BlockAction.GetConnectorActions((IMyShipConnector)TBlock, blockMembers);
            }
            else if (TBlock is IMyParachute)
            {
                BlockAction.GetChuteActions((IMyParachute)TBlock, blockMembers);
            }
        }

        /// <summary>
        /// Custom block actions
        /// </summary>
        private class BlockAction : IBlockMember
        {
            public string Name { get;  }
            public string Value { get { return GetDisplayFunc(); } }
            public bool Enabled { get; }
            public BlockInputType InputType { get; }
            private Action Action { get; set; }
            private Func<string> GetDisplayFunc { get; set; }

            public BlockAction(Func<string> GetDisplayFunc, Action Action)
            {
                Name = "";
                Enabled = true;
                InputType = BlockInputType.None;
                this.GetDisplayFunc = GetDisplayFunc;
                this.Action = Action;
            }

            public void OnSelect() =>
                Action();

            public void OnDeselect()
            { }

            public void HandleInput()
            { }

            /// <summary>
            /// Gets actions for blocks implementing IMyMechanicalConnectionBlock.
            /// </summary>
            public static void GetMechActions(IMyMechanicalConnectionBlock mechBlock, List<IBlockMember> members)
            {
                List<IMyTerminalAction> terminalActions = new List<IMyTerminalAction>();
                mechBlock.GetActions(terminalActions);

                foreach (IMyTerminalAction tAction in terminalActions)
                {
                    string tActionName = tAction.Name.ToString();

                    if (tAction.Id.StartsWith("Add"))
                        members.Add(new BlockAction(
                            () => tActionName,
                            () => tAction.Apply(mechBlock)));
                }

                if (mechBlock is IMyMotorSuspension)
                {
                    members.Add(new BlockAction(
                        () => mechBlock.IsAttached ? "Attach Wheel (Attached)" : "Attach Wheel",
                        () => mechBlock.Attach()));
                    members.Add(new BlockAction(
                        () => "Detach Wheel",
                        () => mechBlock.Detach()));
                }
                else
                {
                    members.Add(new BlockAction(
                        () => mechBlock.IsAttached ? "Attach Head (Attached)" : "Attach Head",
                        () => mechBlock.Attach()));
                    members.Add(new BlockAction(
                        () => "Detach Head",
                        () => mechBlock.Detach()));
                }

                if (mechBlock is IMyPistonBase)
                {
                    IMyPistonBase piston = (IMyPistonBase)mechBlock;

                    members.Add(new BlockAction(
                        () => $"Reverse ({Math.Round(piston.CurrentPosition, 1)}m)",
                        () => piston.Reverse()));
                }
                else if (mechBlock is IMyMotorStator)
                {
                    IMyMotorStator rotor = (IMyMotorStator)mechBlock;

                    members.Add(new BlockAction(
                            () => $"Reverse ({Math.Round(Utils.Math.Clamp(rotor.Angle.RadiansToDegrees(), 0, 359.99))})",
                            () => rotor.TargetVelocityRad = -rotor.TargetVelocityRad));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyDoor.
            /// </summary>
            public static void GetDoorActions(IMyDoor doorBlock, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    () => "Open/Close",
                    () => doorBlock.ToggleDoor()));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(IMyWarhead warhead, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    () => "Start Countdown" + (warhead.IsCountingDown ? $" ({ Math.Truncate(warhead.DetonationTime) })" : ""),
                    () => warhead.StartCountdown()));
                members.Add(new BlockAction(
                    () => "Stop Countdown",
                    () => warhead.StopCountdown()));
                members.Add(new BlockAction(
                    () => "Detonate",
                    () => warhead.Detonate()));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyLandingGear.
            /// </summary>
            public static void GetGearActions(IMyLandingGear landingGear, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
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
            public static void GetConnectorActions(IMyShipConnector connector, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
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
            public static void GetChuteActions(IMyParachute parachute, List<IBlockMember> members)
            {
                members.Add(new BlockAction(
                    () => $"Open/Close ({parachute.Status.ToString()})",
                    () => parachute.ToggleDoor()));
            }
        }

        /// <summary>
        /// Base class for all Build Vision terminal properties that make use of SE's <see cref="ITerminalProperty"/>
        /// </summary>
        private abstract class BvTerminalProperty<T> : IBlockMember where T : ITerminalProperty
        {
            public virtual string Name { get; protected set; }
            public abstract string Value { get; }
            public virtual bool Enabled { get { return control.Enabled(block) && control.Visible(block); } }
            public BlockInputType InputType { get; protected set; }

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

            private const long blinkInterval = 500;
            private readonly Utils.Stopwatch timer;
            private readonly TextInput textInput;
            private bool selected, blink;

            public TextProperty(string name, ITerminalProperty<StringBuilder> textProp, IMyTerminalControl control, IMyTerminalBlock block) : base(name, textProp, control, block)
            {
                InputType |= BlockInputType.Text;
                timer = new Utils.Stopwatch();
                textInput = new TextInput();
            }

            public override void OnSelect()
            {
                textInput.CurrentText = CleanText(property.GetValue(block));
                selected = true;
                timer.Start();
            }

            public override void OnDeselect()
            {
                textInput.Open = false;
                property.SetValue(block, new StringBuilder(textInput.CurrentText));
                selected = false;
                timer.Stop();
            }

            public override void HandleInput()
            {
                textInput.HandleInput();
                textInput.Open = selected && MyAPIGateway.Gui.ChatEntryVisible;

                if (timer.ElapsedMilliseconds > blinkInterval)
                {
                    blink = !blink;
                    timer.Reset();
                }
            }

            private string GetCurrentValue()
            {
                if (selected)
                {
                    if (MyAPIGateway.Gui.ChatEntryVisible)
                    {
                        if (blink)
                            return textInput.CurrentText + '|';
                        else
                            return textInput.CurrentText;
                    }
                    else
                        return "Open Chat to Continue";
                }
                else
                    return CleanText(property.GetValue(block));
            }
        }

        /// <summary>
        /// Block Terminal Property of a Boolean
        /// </summary>
        private class BoolProperty : BvTerminalProperty<ITerminalProperty<bool>>
        {
            public override string Value { get { return GetPropStateText(); } }

            private readonly MyStringId OnText, OffText;

            public BoolProperty(string name, ITerminalProperty<bool> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                if (property is IMyTerminalControlOnOffSwitch)
                {
                    IMyTerminalControlOnOffSwitch onOffSwitch = (IMyTerminalControlOnOffSwitch)property;

                    OnText = onOffSwitch.OnText;
                    OffText = onOffSwitch.OffText;
                }
                else
                {
                    OnText = MySpaceTexts.SwitchText_On;
                    OffText = MySpaceTexts.SwitchText_Off;
                }
            }

            public override void OnSelect()
            {
                property.SetValue(block, !property.GetValue(block));
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
        /// Base for block properties that use scrolling for input.
        /// </summary>
        private abstract class ScrollablePropBase<T> : BvTerminalProperty<T> where T : ITerminalProperty
        {
            protected ScrollablePropBase(string name, T property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                InputType |= BlockInputType.Scroll;
            }

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

        private abstract class NumericPropertyBase<T> : ScrollablePropBase<ITerminalProperty<T>>
        {
            public sealed override string Value => GetDisplay();

            protected readonly Utils.Stopwatch timer;
            protected readonly TextInput textInput;
            protected bool selected, blink;

            public NumericPropertyBase(string name, ITerminalProperty<T> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                InputType |= BlockInputType.Text;
                timer = new Utils.Stopwatch();
                textInput = new TextInput(x => (x >= '0' && x <= '9') || (x == '.' || x == '-' || x == '+'));
            }

            public override void OnSelect()
            {
                textInput.CurrentText = GetValue();
                selected = true;
                timer.Start();
            }

            public override void OnDeselect()
            {
                T newValue;
                textInput.Open = false;

                if (TryParseValue(textInput.CurrentText, out newValue))
                    property.SetValue(block, newValue);

                selected = false;
                timer.Stop();
            }

            protected abstract bool TryParseValue(string text, out T value);

            public override void HandleInput()
            {
                base.HandleInput();

                textInput.HandleInput();
                textInput.Open = selected && MyAPIGateway.Gui.ChatEntryVisible;

                if (timer.ElapsedMilliseconds > 500)
                {
                    blink = !blink;
                    timer.Reset();
                }
            }

            protected virtual string GetDisplay()
            {
                if (selected && MyAPIGateway.Gui.ChatEntryVisible)
                {
                    if (blink)
                        return textInput.CurrentText + '|';
                    else
                        return textInput.CurrentText;
                }

                return GetValue();
            }

            protected virtual string GetValue() =>
                property.GetValue(block).ToString();
        }

        /// <summary>
        /// Block Terminal Property of a Float
        /// </summary>
        private class FloatProperty : NumericPropertyBase<float>
        {
            private readonly float minValue, maxValue, incrX, incrY, incrZ, incr0;

            public FloatProperty(string name, ITerminalProperty<float> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                minValue = this.property.GetMinimum(block);
                maxValue = this.property.GetMaximum(block);

                if (property.Id.StartsWith("Rot"))
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

            protected override bool TryParseValue(string text, out float value) =>
                float.TryParse(text, out value);

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
        private class ColorProperty : NumericPropertyBase<Color>
        {
            private readonly int channel;
            private static int incrX, incrY, incrZ, incr0;

            public ColorProperty(string name, ITerminalProperty<Color> property, IMyTerminalControl control, IMyTerminalBlock block, int channel)
                : base(name, property, control, block)
            {
                InputType |= BlockInputType.Text;

                incr0 = 1;
                incrZ = (incr0 * Cfg.colorMult.Z); // x64
                incrY = (incr0 * Cfg.colorMult.Y); // x16
                incrX = (incr0 * Cfg.colorMult.X); // x8

                this.channel = channel;
            }

            /// <summary>
            /// Returns a scrollable property for each color channel in an ITerminalProperty<Color> object
            /// </summary>
            public static ColorProperty[] GetColorProperties(string name, ITerminalProperty<Color> property, IMyTerminalControl control, IMyTerminalBlock block)
            {
                return new ColorProperty[]
                {
                    new ColorProperty($"{name}: R", property, control, block, 0),
                    new ColorProperty($"{name}: G", property, control, block, 1),
                    new ColorProperty($"{name}: B", property, control, block, 2)
                };
            }

            protected override void ScrollDown() =>
                SetPropValue(false);

            protected override void ScrollUp() =>
                SetPropValue(true);

            protected override bool TryParseValue(string text, out Color value)
            {
                byte x;
                value = property.GetValue(block);

                if (byte.TryParse(text, out x))
                {
                    value = value.SetChannel(channel, x);
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Changes property color value based on given color delta.
            /// </summary>
            private void SetPropValue(bool increment)
            {
                Color current = property.GetValue(block);
                int value = current.GetChannel(channel),
                    mult = increment ? GetIncrement() : -GetIncrement();

                current = current.SetChannel(channel, (byte)(value + mult));
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

            protected override string GetValue() =>
                property.GetValue(block).GetChannel(channel).ToString();
        }
    }
}