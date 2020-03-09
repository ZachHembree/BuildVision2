using RichHudFramework;
using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Game.Components;
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
        /// Retrieves the name of the block property
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Retrieves the current value of the block member as a <see cref="string"/>
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Additional information following the value of the member.
        /// </summary>
        string Postfix { get; }

        /// <summary>
        /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
        /// </summary>
        bool Enabled { get; }
    }

    internal interface IBlockAction : IBlockMember
    {
        void Action();
    }

    internal interface IBlockTextMember : IBlockMember
    {
        Func<char, bool> CharFilterFunc { get; }
        void SetValueText(string text);
    }

    internal interface IBlockScrollable : IBlockMember
    {
        void ScrollUp();

        void ScrollDown();
    }

    /// <summary>
    /// Encapsulates all block property data needed for the UI.
    /// </summary>
    internal class PropertyBlock
    {
        public IMyTerminalBlock TBlock { get; private set; }
        public ReadOnlyCollection<IBlockMember> BlockMembers { get; }
        public int EnabledMembers => GetEnabledElementCount();
        public bool IsFunctional { get { return TBlock.IsFunctional; } }
        public bool IsWorking { get { return TBlock.IsWorking; } }
        public bool CanLocalPlayerAccess { get { return TBlock.HasLocalPlayerAccess(); } }
        public readonly Vector3D modelOffset;
        public static PropBlockConfig Cfg { get { return BvConfig.Current.block; } set { BvConfig.Current.block = value; } }

        private readonly List<IBlockMember> blockMembers;
        private readonly List<BvTerminalPropertyBase> blockProperties;

        public PropertyBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;
            blockMembers = new List<IBlockMember>();
            blockProperties = new List<BvTerminalPropertyBase>();
            BlockMembers = new ReadOnlyCollection<IBlockMember>(blockMembers);

            GetScrollableProps();
            GetScrollableActions();

            BoundingBoxD bb;
            tBlock.SlimBlock.GetWorldBoundingBox(out bb);
            modelOffset = bb.Center - tBlock.GetPosition();
        }

        /// <summary>
        /// Gets the block's current position.
        /// </summary>
        public Vector3D GetPosition() =>
            TBlock.GetPosition();

        public void CopySettings(PropertyBlock src)
        {
            for (int n = 0; n < src.blockProperties.Count; n++)
            {
                BvTerminalPropertyBase dest = blockProperties.Find(x => x.Id == src.blockProperties[n].Id);

                if (dest != null)
                    dest.TryCopyProperty(src.blockProperties[n]);
            }
        }

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
            if (text != null)
            {
                StringBuilder cleanedText = new StringBuilder(text.Length);

                for (int n = 0; n < text.Length; n++)
                {
                    if (text[n] > 31)
                        cleanedText.Append(text[n]);
                }

                return cleanedText.ToString();
            }
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
                if (prop is IMyTerminalControl)
                {
                    IMyTerminalControl control = (IMyTerminalControl)prop;
                    name = GetTooltipName(prop);

                    if (name.Length > 0)
                    {
                        if (prop is ITerminalProperty<StringBuilder>)
                        {
                            ITerminalProperty<StringBuilder> textProp = (ITerminalProperty<StringBuilder>)prop;

                            if (prop.Id == "Name")
                                blockProperties.Insert(0, new TextProperty(name, textProp, control, TBlock));
                            else
                                blockProperties.Add(new TextProperty(name, textProp, control, TBlock));
                        }
                        if (prop is IMyTerminalControlCombobox) // fields having to do with camera assignments seem to give me trouble here
                        {
                            try
                            {
                                blockProperties.Add(new ComboBoxProperty(name, (IMyTerminalControlCombobox)prop, control, TBlock));
                            }
                            catch { }
                        }
                        else if (prop is ITerminalProperty<bool>)
                        {
                            blockProperties.Add(new BoolProperty(name, (ITerminalProperty<bool>)prop, control, TBlock));
                        }
                        else if (prop is ITerminalProperty<float>)
                        {
                            blockProperties.Add(new FloatProperty(name, (ITerminalProperty<float>)prop, control, TBlock));
                        }
                        else if (prop is ITerminalProperty<Color>)
                        {
                            blockProperties.AddRange(ColorProperty.GetColorProperties(name, (ITerminalProperty<Color>)prop, control, TBlock));
                        }
                    }
                }
            }

            blockMembers.AddRange(blockProperties);
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

        private abstract class BlockMemberBase : IBlockMember
        {
            public virtual string Name { get; protected set; }
            public abstract string Value { get; }
            public abstract string Postfix { get; }
            public virtual bool Enabled { get; protected set; }
        }

        /// <summary>
        /// Custom block actions
        /// </summary>
        private class BlockAction : BlockMemberBase, IBlockAction
        {
            public override string Value => GetSafeValueFunc();
            public override string Postfix => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private Func<string> GetSafeValueFunc { get; set; }
            private Func<string> GetPostfixFunc { get; set; }
            private readonly Action action;

            public BlockAction(Func<string> GetSafeValueFunc, Func<string> GetPostfixFunc, Action Action)
            {
                Name = null;
                Enabled = true;

                this.GetSafeValueFunc = GetSafeValueFunc;
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

        private abstract class BvTerminalPropertyBase : BlockMemberBase
        {
            public abstract string Id { get; }

            public abstract bool TryCopyProperty(BvTerminalPropertyBase prop);
        }

        /// <summary>
        /// Base class for all Build Vision terminal properties that make use of SE's <see cref="ITerminalProperty"/>
        /// </summary>
        private abstract class BvTerminalProperty<TProp> : BvTerminalPropertyBase where TProp : ITerminalProperty
        {
            public override string Id => property.Id;
            public override bool Enabled { get { return control.Enabled(block) && control.Visible(block); } }

            protected readonly TProp property;
            protected readonly IMyTerminalControl control;
            protected readonly IMyTerminalBlock block;

            protected BvTerminalProperty(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block)
            {
                Name = name;

                this.property = property;
                this.control = control;
                this.block = block;
            }
        }

        private abstract class BvTerminalProperty<TProp, TValue> : BvTerminalProperty<TProp> where TProp : ITerminalProperty<TValue>
        {
            protected BvTerminalProperty(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            { }

            public TValue GetValue()
            {
                try
                {
                    // Because some custom blocks don't have their properties set up correctly
                    // and I have to live with it
                    if (control.Enabled(block) && control.Visible(block))
                        return property.GetValue(block);
                }
                catch
                { }

                return default(TValue);
            }

            public void SetValue(TValue value)
            {
                try
                {
                    if (control.Enabled(block) && control.Visible(block))
                        property.SetValue(block, value);
                }
                catch { }
            }
        }

        private abstract class BvTerminalValueControl<TProp, TValue> : BvTerminalProperty<TProp> where TProp : IMyTerminalValueControl<TValue>
        {
            protected BvTerminalValueControl(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            { }

            public TValue GetValue()
            {
                try
                {
                    if (control.Enabled(block) && control.Visible(block))
                        return property.Getter(block);
                }
                catch
                { }

                return default(TValue);
            }

            public void SetValue(TValue value)
            {
                try
                {
                    if (control.Enabled(block) && control.Visible(block))
                        property.Setter(block, value);
                }
                catch { }
            }
        }

        /// <summary>
        /// Field for changing block property text. 
        /// </summary>
        private class TextProperty : BvTerminalProperty<ITerminalProperty<StringBuilder>, StringBuilder>, IBlockTextMember
        {
            public override string Value => CleanText(GetValue());
            public override string Postfix => null;
            public Func<char, bool> CharFilterFunc { get; protected set; }

            public TextProperty(string name, ITerminalProperty<StringBuilder> textProp, IMyTerminalControl control, IMyTerminalBlock block) : base(name, textProp, control, block)
            {
                CharFilterFunc = x => (x >= ' ');
            }

            public void SetValueText(string text)
            {
                SetValue(new StringBuilder(text));
            }

            public override bool TryCopyProperty(BvTerminalPropertyBase prop)
            {
                var x = prop as TextProperty;

                if (x != null)
                {
                    SetValue(x.GetValue());

                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Block Terminal Property of a Boolean
        /// </summary>
        private class BoolProperty : BvTerminalProperty<ITerminalProperty<bool>, bool>, IBlockAction
        {
            public override string Value => GetPropStateText();
            public override string Postfix => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private readonly Func<string> GetPostfixFunc;
            private readonly MyStringId OnText, OffText;

            public BoolProperty(string name, ITerminalProperty<bool> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                if (property.Id == "OnOff" && (block.ResourceSink != null || block is IMyPowerProducer)) // Insert power draw / output info
                {
                    MyDefinitionId definitionId = MyDefinitionId.FromContent(block.SlimBlock.GetObjectBuilder());
                    var sink = block.ResourceSink;
                    var producer = block as IMyPowerProducer;

                    GetPostfixFunc = () => GetBlockPowerInfo(sink, producer, definitionId);
                }
                else if (property.Id == "Stockpile" && block is IMyGasTank) // Insert gas tank info
                {
                    GetPostfixFunc = () => GetGasTankFillPercent((IMyGasTank)block);
                }

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

            private static string GetBlockPowerInfo(MyResourceSinkComponentBase sink, IMyPowerProducer producer, MyDefinitionId definitionId)
            {
                string disp = "", suffix;
                float powerDraw = sink != null ? sink.CurrentInputByType(definitionId) : 0f,
                    powerOut = producer != null ? producer.CurrentOutput : 0f,
                    total = (powerDraw + powerOut), scale;

                if (total >= 1000f)
                {
                    scale = .001f;
                    suffix = "GW";
                }
                else if (total >= 1f)
                {
                    scale = 1f;
                    suffix = "MW";
                }
                else if (total >= .001f)
                {
                    scale = 1000f;
                    suffix = "KW";
                }
                else
                {
                    scale = 1000000f;
                    suffix = "W";
                }

                if (sink != null)
                    disp += "-" + Math.Round(powerDraw * scale, 1);

                if (producer != null)
                {
                    if (sink != null)
                        disp += " / ";

                    disp += "+" + Math.Round(powerOut * scale, 1);
                }

                return $"({disp} {suffix})";
            }

            private static string GetGasTankFillPercent(IMyGasTank gasTank)
            {
                return $"({Math.Round(gasTank.FilledRatio * 100d, 1)}%)";
            }

            public void Action()
            {
                SetValue(!GetValue());
            }

            public override bool TryCopyProperty(BvTerminalPropertyBase prop)
            {
                if (prop.GetType() == this.GetType())
                {
                    var x = prop as BoolProperty;
                    SetValue(x.GetValue());

                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Retrieves the on/off state of given property of a given block as a string.
            /// </summary>
            private string GetPropStateText()
            {
                if (GetValue())
                    return MyTexts.Get(OnText).ToString();
                else
                    return MyTexts.Get(OffText).ToString();
            }
        }

        /// <summary>
        /// Base for block properties that use scrolling for input.
        /// </summary>
        private abstract class ScrollableProp<TProp> : BvTerminalProperty<TProp>, IBlockScrollable where TProp : ITerminalProperty
        {
            protected ScrollableProp(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            { }

            public abstract void ScrollUp();

            public abstract void ScrollDown();
        }

        private abstract class ScrollableProp<TProp, TValue> : BvTerminalProperty<TProp, TValue>, IBlockScrollable where TProp : ITerminalProperty<TValue>
        {
            protected ScrollableProp(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            { }

            public abstract void ScrollUp();

            public abstract void ScrollDown();
        }

        private abstract class ScrollableValueControl<TProp, TValue> : BvTerminalValueControl<TProp, TValue>, IBlockScrollable where TProp : IMyTerminalValueControl<TValue>
        {
            protected ScrollableValueControl(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            { }

            public abstract void ScrollUp();

            public abstract void ScrollDown();
        }

        /// <summary>
        /// Scrollable property for <see cref="IMyTerminalControlCombobox"/> terminal properties.
        /// </summary>
        private class ComboBoxProperty : ScrollableValueControl<IMyTerminalControlCombobox, long>
        {
            public override string Value => GetValueFunc();
            public override string Postfix => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private readonly List<long> keys;
            private readonly List<string> names;
            private readonly Func<string> GetValueFunc, GetPostfixFunc;

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

                if (control.Id == "ChargeMode" && block is IMyBatteryBlock) // Insert bat charge info
                {
                    var bat = (IMyBatteryBlock)block;
                    GetPostfixFunc = () => $"({Math.Round((bat.CurrentStoredPower / bat.MaxStoredPower) * 100f, 1)}%)";
                }

                GetValueFunc = () => names[GetCurrentIndex()];
            }

            public override void ScrollUp() =>
                ChangePropValue(1);

            public override void ScrollDown() =>
                ChangePropValue(-1);

            private void ChangePropValue(int delta)
            {
                int index = MathHelper.Clamp((GetCurrentIndex() + delta), 0, keys.Count - 1);
                SetValue(keys[index]);
            }

            private int GetCurrentIndex()
            {
                long key = GetValue();
                
                for (int n = 0; n < keys.Count; n++)
                {
                    if (keys[n] == key)
                        return n;
                }

                return 0;
            }

            public override bool TryCopyProperty(BvTerminalPropertyBase prop)
            {
                var x = prop as ComboBoxProperty;

                if (x != null)
                {
                    SetValue(x.GetValue());
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Property for numerical values. Allows scrolling and text input.
        /// </summary>
        private abstract class NumericPropertyBase<TValue> : ScrollableProp<ITerminalProperty<TValue>, TValue>, IBlockTextMember
        {
            public Func<char, bool> CharFilterFunc { get; protected set; }

            public NumericPropertyBase(string name, ITerminalProperty<TValue> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                CharFilterFunc = x => (x >= '0' && x <= '9') || x == '.' || x == '-' || x == '+';
            }

            public void SetValueText(string value)
            {
                TValue newValue;

                if (TryParseValue(value, out newValue))
                    SetValue(newValue);
            }

            protected abstract bool TryParseValue(string text, out TValue value);

            public override bool TryCopyProperty(BvTerminalPropertyBase prop)
            {
                var x = prop as NumericPropertyBase<TValue>;

                if (x != null)
                {
                    SetValue(x.GetValue());

                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Block Terminal Property of a Float
        /// </summary>
        private class FloatProperty : NumericPropertyBase<float>
        {
            public override string Value
            {
                get
                {
                    float value = GetValue();

                    if ((value.Abs() >= 1000000f || value.Abs() <= .0000001f) && value != 0f)
                        return value.ToString("0.##E+0");
                    else
                        return value.ToString("0.##");
                }
            }
            public override string Postfix => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private readonly float minValue, maxValue, incrX, incrY, incrZ, incr0;
            private readonly Func<string> GetPostfixFunc;

            public FloatProperty(string name, ITerminalProperty<float> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                minValue = property.GetMinimum(block);
                maxValue = property.GetMaximum(block);

                if (property.Id.StartsWith("Rot")) // Increment exception for projectors
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

                if (property.Id == "UpperLimit")
                {
                    if (block is IMyPistonBase)
                    {
                        var piston = (IMyPistonBase)block;
                        GetPostfixFunc = () => $"({Math.Round(piston.CurrentPosition, 1)}m)";
                    }
                    else if (block is IMyMotorStator)
                    {
                        var rotor = (IMyMotorStator)block;
                        GetPostfixFunc = () => $"({Math.Round(MathHelper.Clamp(rotor.Angle.RadiansToDegrees(), -360, 360))})";
                    }
                }
            }

            public override void ScrollDown() =>
                ChangePropValue(-GetIncrement());

            public override void ScrollUp() =>
                ChangePropValue(+GetIncrement());

            protected override bool TryParseValue(string text, out float value) =>
                float.TryParse(text, out value);

            /// <summary>
            /// Changes property float value based on given delta.
            /// </summary>
            private void ChangePropValue(float delta)
            {
                float current = GetValue();

                if (float.IsInfinity(current))
                    current = 0f;

                SetValue((float)Math.Round(MathHelper.Clamp((current + delta), minValue, maxValue), 3));
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private float GetIncrement()
            {
                if (BvBinds.MultZ.IsPressed)
                    return incrZ;
                else if (BvBinds.MultY.IsPressed)
                    return incrY;
                else if (BvBinds.MultX.IsPressed)
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
            public override string Value => GetValue().GetChannel(channel).ToString();
            public override string Postfix => null;

            private readonly int channel;
            private static int incrX, incrY, incrZ, incr0;

            public ColorProperty(string name, ITerminalProperty<Color> property, IMyTerminalControl control, IMyTerminalBlock block, int channel)
                : base(name, property, control, block)
            {
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

            public override void ScrollDown() =>
                SetPropValue(false);

            public override void ScrollUp() =>
                SetPropValue(true);

            protected override bool TryParseValue(string text, out Color value)
            {
                byte x;
                value = GetValue();

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
                Color current = GetValue();
                int value = current.GetChannel(channel),
                    mult = increment ? GetIncrement() : -GetIncrement();

                current = current.SetChannel(channel, (byte)MathHelper.Clamp(value + mult, 0, 255));
                SetValue(current);
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private int GetIncrement()
            {
                if (BvBinds.MultZ.IsPressed)
                    return incrZ;
                else if (BvBinds.MultY.IsPressed)
                    return incrY;
                else if (BvBinds.MultX.IsPressed)
                    return incrX;
                else
                    return incr0;
            }
        }        
    }
}