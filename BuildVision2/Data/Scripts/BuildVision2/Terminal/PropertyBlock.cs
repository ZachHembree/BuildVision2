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

namespace DarkHelmet.BuildVision2
{
    internal interface IScrollableElement
    {
        /// <summary>
        /// Retrieves the block element name and current value.
        /// </summary>
        string Display { get; }
    }

    internal interface IScrollableAction : IScrollableElement
    {
        /// <summary>
        /// Triggers action associated with block;
        /// </summary>
        void Action();
    }

    internal interface IScrollableProp : IScrollableElement
    {
        /// <summary>
        /// Retrieves the name of the block
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Retrieves the current value of the block property as a string
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Decreases block property value each time its called.
        /// </summary>
        void ScrollDown();

        /// <summary>
        /// Increases block property value each time its called.
        /// </summary>
        void ScrollUp();
    }

    /// <summary>
    /// Encapsulates all block property data needed for the UI.
    /// </summary>
    internal class PropertyBlock
    {
        public IMyTerminalBlock TBlock { get; private set; }
        public List<IScrollableAction> Actions { get; private set; }
        public List<IScrollableProp> Properties { get; private set; }
        public int ScrollableCount { get; private set; }
        public bool IsFunctional { get { return TBlock.IsFunctional; } }
        public bool IsWorking { get { return TBlock.IsWorking; } }
        public bool CanLocalPlayerAccess { get { return TBlock.HasLocalPlayerAccess(); } }
        public static PropBlockConfig Cfg { get { return BvConfig.Current.propertyBlock; } set { BvConfig.Current.propertyBlock = value; } }

        public PropertyBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;

            Actions = GetScrollableActions();
            Properties = GetScrollableProps();
            ScrollableCount = Actions.Count + Properties.Count;
        }

        /// <summary>
        /// Gets the block's current position.
        /// </summary>
        public Vector3D GetPosition() =>
            TBlock.GetPosition();

        /// <summary>
        /// Retrieves a set of custom block actions.
        /// </summary>
        private List<IScrollableAction> GetScrollableActions()
        {
            List<IScrollableAction> actions = new List<IScrollableAction>();

            if (TBlock is IMyMechanicalConnectionBlock)
            {
                BlockAction.GetMechActions(TBlock, actions);
            }
            else if (TBlock is IMyDoor)
            {
                BlockAction.GetDoorActions(TBlock, actions);
            }
            else if (TBlock is IMyWarhead)
            {
                BlockAction.GetWarheadActions(TBlock, actions);
            }
            else if (TBlock is IMyLandingGear)
            {
                BlockAction.GetGearActions(TBlock, actions);
            }
            else if (TBlock is IMyShipConnector)
            {
                BlockAction.GetConnectorActions(TBlock, actions);
            }
            else if (TBlock is IMyParachute)
            {
                BlockAction.GetChuteActions(TBlock, actions);
            }

            return actions;
        }

        /// <summary>
        /// Retrieves all block ITerminalProperty values.
        /// </summary>
        private List<IScrollableProp> GetScrollableProps()
        {
            List<ITerminalProperty> properties = new List<ITerminalProperty>(12);
            List<IScrollableProp> scrollables;
            IMyTerminalControl terminalControl;
            string name;

            TBlock.GetProperties(properties);
            scrollables = new List<IScrollableProp>(properties.Count);

            foreach (ITerminalProperty prop in properties)
            {
                name = GetTooltipName(prop);
                terminalControl = prop as IMyTerminalControl;

                if (name.Length > 0 && terminalControl != null && terminalControl.Visible(TBlock))
                {
                    if (prop is ITerminalProperty<bool>)
                    {
                        scrollables.Add(new BoolProperty(name, (ITerminalProperty<bool>)prop, this));
                    }
                    else if (prop is ITerminalProperty<float>)
                    {
                        scrollables.Add(new FloatProperty(name, (ITerminalProperty<float>)prop, this));
                    }
                    else if (prop is ITerminalProperty<Color>)
                    {
                        scrollables.AddRange(ColorProperty.GetColorProperties(name, this, (ITerminalProperty<Color>)prop));
                    }
                }
            }

            if (TBlock is IMyBatteryBlock)
                scrollables.Add(new BattProperty((IMyBatteryBlock)TBlock));

            return scrollables;
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
        private class BlockAction : IScrollableAction
        {
            public string Display { get { return GetDisplay(); } }
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
            public static void GetMechActions(IMyTerminalBlock tBlock, List<IScrollableAction> actions)
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
                        () => "Attach Wheel",
                        () => mechBlock.Attach()));
                    actions.Add(new BlockAction(
                        () => mechBlock.IsAttached ? "Detach Wheel (Ready)" : "Detach Wheel",
                        () => mechBlock.Detach()));
                }
                else
                {
                    actions.Add(new BlockAction(
                        () => "Attach Head",
                        () => mechBlock.Attach()));
                    actions.Add(new BlockAction(
                        () => mechBlock.IsAttached ? "Detach Head (Ready)" : "Detach Head",
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
            public static void GetDoorActions(IMyTerminalBlock tBlock, List<IScrollableAction> actions)
            {
                IMyDoor doorBlock = (IMyDoor)tBlock;

                actions.Add(new BlockAction(
                    () => "Open/Close",
                    () => doorBlock.ToggleDoor()));
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(IMyTerminalBlock tBlock, List<IScrollableAction> actions)
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
            public static void GetGearActions(IMyTerminalBlock tBlock, List<IScrollableAction> actions)
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
            public static void GetConnectorActions(IMyTerminalBlock tBlock, List<IScrollableAction> actions)
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
            public static void GetChuteActions(IMyTerminalBlock tBlock, List<IScrollableAction> actions)
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
        /// Scrollable property for battery charge modes
        /// </summary>
        private class BattProperty : IScrollableProp
        {
            public string Display { get { return Name + ": " + Value; } }
            public string Name { get; } = "Charge Mode";
            public string Value { get { return GetChargeModeName(); } }

            private readonly IMyBatteryBlock battery;
            private int index;

            public BattProperty(IMyBatteryBlock battery)
            {
                this.battery = battery;
                index = GetChargeModeIndex();
            }

            public void ScrollDown() =>
                ChangeChargeMode(true);

            public void ScrollUp() =>
                ChangeChargeMode(false);

            private void ChangeChargeMode(bool scrollUp)
            {
                index = GetChargeModeIndex();

                if (scrollUp)
                    index--;
                else
                    index++;

                index = Utils.Math.Clamp(index, 1, 3);

                if (index == 1)
                    battery.ChargeMode = ChargeMode.Auto;
                else if (index == 2)
                    battery.ChargeMode = ChargeMode.Discharge;
                else if (index == 3)
                    battery.ChargeMode = ChargeMode.Recharge;
            }

            private int GetChargeModeIndex()
            {
                if (battery.ChargeMode == ChargeMode.Auto)
                    return 1;
                else if (battery.ChargeMode == ChargeMode.Discharge)
                    return 2;
                else if (battery.ChargeMode == ChargeMode.Recharge)
                    return 3;
                else
                    return 0;
            }

            private string GetChargeModeName()
            {
                if (index == 1)
                    return "Auto";
                else if (index == 2)
                    return "Discharge";
                else if (index == 3)
                    return "Recharge";
                else
                    return "null";
            }
        }

        /// <summary>
        /// Block Terminal Property of a Boolean
        /// </summary>
        private class BoolProperty : IScrollableProp
        {
            public string Name { get; private set; }
            public string Value { get { return GetPropStateText(); } }
            public string Display { get { return Name + ": " + Value; } }

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

            public void ScrollDown()
            {
                if (prop.GetValue(pBlock.TBlock) == true)
                    prop.SetValue(pBlock.TBlock, false);
            }

            public void ScrollUp()
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
        private class FloatProperty : IScrollableProp
        {
            public string Name { get; private set; }
            public string Value { get { return prop.GetValue(pBlock.TBlock).ToString(); } }
            public string Display { get { return Name + ": " + Value; } }

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

                incrC = incr0 * Cfg.floatMult.Z; // x64
                incrB = incr0 * Cfg.floatMult.Y; // x16
                incrA = incr0 * Cfg.floatMult.X; // x8
            }

            public void ScrollDown() =>
                ChangePropValue(-GetIncrement());

            public void ScrollUp() =>
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
        private class ColorProperty : IScrollableProp
        {
            public string Name { get; private set; }
            public string Value { get { return colorDisp(); } }
            public string Display { get { return Name + colorDisp(); } }

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
            public static IScrollableProp[] GetColorProperties(string name, PropertyBlock pBlock, ITerminalProperty<Color> prop)
            {
                return new IScrollableProp[]
                {
                    new ColorProperty(name, pBlock, prop, new Color(1, 0, 0), () => (" R: " + prop.GetValue(pBlock.TBlock).R)), // R
                    new ColorProperty(name, pBlock, prop, new Color(0, 1, 0), () => (" G: " + prop.GetValue(pBlock.TBlock).G)), // G
                    new ColorProperty(name, pBlock, prop, new Color(0, 0, 1), () => (" B: " + prop.GetValue(pBlock.TBlock).B))  // B
                };
            }

            public void ScrollDown() =>
                ChangePropValue(false);

            public void ScrollUp() =>
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