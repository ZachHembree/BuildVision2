using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Utils;
using VRageMath;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DoorStatus = Sandbox.ModAPI.Ingame.DoorStatus;
using ChargeMode = Sandbox.ModAPI.Ingame.ChargeMode;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;
using IMyBatteryBlock = Sandbox.ModAPI.Ingame.IMyBatteryBlock;
using IMyParachute = SpaceEngineers.Game.ModAPI.Ingame.IMyParachute;
using IMyTerminalAction = Sandbox.ModAPI.Interfaces.ITerminalAction;
using IMyTextSurfaceProvider = Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider;
using IMyCockpit = Sandbox.ModAPI.IMyCockpit;

namespace DarkHelmet.BuildVision2
{
    internal interface IScrollableAction
    {
        /// <summary>
        /// Retrieves the block action name and current value.
        /// </summary>
        Func<string> GetDisplay { get; }
        
        /// <summary>
        /// Triggers action associated with block;
        /// </summary>
        Action Action { get; }
    }
    
    internal interface IScrollableProp
    {
        /// <summary>
        /// Retrieves the block property name and current value.
        /// </summary>
        string GetDisplay();

        /// <summary>
        /// Retrieves the name of the block
        /// </summary>
        string GetName();

        /// <summary>
        /// Retrieves the current value of the block property as a string
        /// </summary>
        string GetValue();

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
    /// Stores configuration of scrollable data for serializatin.
    /// </summary>
    public class PropBlockConfig
    {
        [XmlIgnore]
        public static PropBlockConfig Defaults
        {
            get
            {
                return new PropBlockConfig
                {
                    floatDiv = 1000.0,
                    colorDiv = 256,
                    floatMult = new Vector3(10f, 25f, 100f),
                    colorMult = new Vector3I(8, 16, 64)
                };
            }
        }

        [XmlElement(ElementName = "FloatIncrementDivisor")]
        public double floatDiv;

        [XmlElement(ElementName = "ColorIncrementDivisor")]
        public int colorDiv;

        [XmlElement(ElementName = "FloatPropertyMultipliers")]
        public Vector3 floatMult;

        [XmlElement(ElementName = "ColorPropertyMultipliers")]
        public Vector3I colorMult;

        /// <summary>
        /// Checks for any fields that have invalid values and resets them to the default if necessary.
        /// </summary>
        public void Validate()
        {
            PropBlockConfig defaults = Defaults;
            
            if (floatDiv == default(double))
                floatDiv = defaults.floatDiv;

            if (colorDiv == default(int))
                colorDiv = defaults.colorDiv;

            // Float multipliers
            if (floatMult.X == default(float))
                floatMult.X = defaults.floatMult.X;

            if (floatMult.Y == default(float))
                floatMult.Y = defaults.floatMult.Y;

            if (floatMult.Z == default(float))
                floatMult.Z = defaults.floatMult.Z;

            // Color multipiers
            if (colorMult.X == default(int))
                colorMult.X = defaults.colorMult.X;

            if (colorMult.Y == default(int))
                colorMult.Y = defaults.colorMult.Y;

            if (colorMult.Z == default(int))
                colorMult.Z = defaults.colorMult.Z;
        }
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
        public bool IsMechConnection { get; private set; }
        public bool IsFunctional { get { return TBlock.IsFunctional; } }
        public bool CanLocalPlayerAccess { get { return TBlock.HasLocalPlayerAccess(); } }

        private static Binds Binds { get { return Binds.Instance; } }
        private static PropBlockConfig cfg;

        static PropertyBlock()
        {
            UpdateConfig(PropBlockConfig.Defaults);
        }

        public PropertyBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;
            IsMechConnection = false;

            Actions = GetScrollableActions();
            Properties = GetScrollableProps();
            ScrollableCount = Actions.Count + Properties.Count;
        }
        
        /// <summary>
        /// Updates the current configuration.
        /// </summary>
        public static void UpdateConfig(PropBlockConfig cfg)
        {
            cfg.Validate();
            PropertyBlock.cfg = cfg;
        }

        /// <summary>
        /// Returns the current configuration.
        /// </summary>
        public static PropBlockConfig GetConfig()
        {
            return cfg;
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
                IsMechConnection = true;
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
            IMyBatteryBlock battery = TBlock as IMyBatteryBlock;
            string name;

            TBlock.GetProperties(properties);
            scrollables = new List<IScrollableProp>(properties.Count);

            foreach (ITerminalProperty prop in properties)
            {
                name = GetTooltipName(prop);

                // if Attributes.IsDefined were whitelisted, this would be a lot less dumb
                if (name.Length > 0 && !(IsMechConnection && name.StartsWith("Safety")))
                {
                    if (prop is ITerminalProperty<bool>)
                        scrollables.Add(new BoolProperty(name, prop.AsBool(), this));
                    else if (prop is ITerminalProperty<float>)
                        scrollables.Add(new FloatProperty(name, prop.AsFloat(), this));
                    else if (prop is ITerminalProperty<Color>)
                    {
                        try
                        {
                            ITerminalProperty<Color> color = prop.AsColor();
                            color.GetValue(TBlock);
                            scrollables.AddRange(ColorProperty.GetColorProperties(name, this, color));
                        }
                        catch
                        {
                            //arrrggh
                        }
                    }
                }
            }

            if (battery != null)
                scrollables.Add(new BattProperty(battery));

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
            public Func<string> GetDisplay { get; private set; }
            public Action Action { get; private set; }

            public BlockAction(Func<string> GetDisplay, Action Action)
            {
                this.GetDisplay = GetDisplay;
                this.Action = Action;
            }

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

                foreach (IMyTerminalAction tAction in terminalActions) // sketchy, but I've done worse already
                {
                    string tActionName = tAction.Name.ToString();

                    if (tAction.Name.ToString().StartsWith("Add "))
                        actions.Add(new BlockAction(
                            () => tActionName,
                            () => tAction.Apply(mechBlock)));
                }

                actions.Add(new BlockAction(
                    () => "Attach Head",
                    () => mechBlock.Attach()));
                actions.Add(new BlockAction(
                    () => mechBlock.IsAttached ? "Detach Head (Ready)" : "Detach Head",
                    () => mechBlock.Detach()));

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
            private readonly IMyBatteryBlock battery;
            private readonly string name;
            private int index;

            public BattProperty(IMyBatteryBlock battery)
            {
                this.battery = battery;
                name = "Charge Mode ";
                index = GetChargeModeIndex();
            }

            public string GetDisplay() =>
                name + ": " + GetChargeModeName();

            public string GetName() =>
                name;

            public string GetValue() =>
                GetChargeModeName();

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

                index= Utilities.Clamp(index, 1, 3);

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
            private readonly PropertyBlock pBlock;
            private readonly ITerminalProperty<bool> prop;
            private readonly string name;

            public BoolProperty(string name, ITerminalProperty<bool> prop, PropertyBlock block)
            {
                this.prop = prop;
                this.pBlock = block;
                this.name = name;
            }

            public string GetDisplay() =>
                name + ": " + GetPropStateText();

            public string GetName() =>
                name;

            public string GetValue() =>
                GetPropStateText();

            public void ScrollDown() =>
                prop.SetValue(pBlock.TBlock, false);

            public void ScrollUp() =>
                prop.SetValue(pBlock.TBlock, true);

            /// <summary>
            /// Retrieves the on/off state of given property of a given block as a string.
            /// </summary>
            private string GetPropStateText()
            {
                if (prop.GetValue(pBlock.TBlock))
                    return MyTexts.Get(MySpaceTexts.SwitchText_On).ToString();
                else
                    return MyTexts.Get(MySpaceTexts.SwitchText_Off).ToString();
            }
        }

        /// <summary>
        /// Block Terminal Property of a Float
        /// </summary>
        private class FloatProperty : IScrollableProp
        {
            private readonly PropertyBlock pBlock;
            private readonly ITerminalProperty<float> prop;
            private readonly string name;
            private float minValue, maxValue, incrA, incrB, incrC, incr0;

            public FloatProperty(string name, ITerminalProperty<float> prop, PropertyBlock block)
            {
                this.prop = prop;
                this.pBlock = block;
                this.name = name;
                minValue = this.prop.GetMinimum(pBlock.TBlock);
                maxValue = this.prop.GetMaximum(pBlock.TBlock);

                if (float.IsInfinity(minValue))
                    minValue = -1000f;

                if (float.IsInfinity(maxValue))
                    maxValue = 1000;

                incr0 = Utilities.Clamp((float)Math.Round(maxValue / cfg.floatDiv, 1), .1f, float.PositiveInfinity);
                incrC = incr0 * cfg.floatMult.Z; // x64
                incrB = incr0 * cfg.floatMult.Y; // x16
                incrA = incr0 * cfg.floatMult.X; // x8
            }

            public string GetDisplay() =>
                prop.Id + ": " + prop.GetValue(pBlock.TBlock);

            public string GetName() =>
                prop.Id;

            public string GetValue() =>
                prop.GetValue(pBlock.TBlock).ToString();

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

                prop.SetValue(pBlock.TBlock, (float)Math.Round(Utilities.Clamp((current + delta), minValue, maxValue), 1));
            }
            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private float GetIncrement()
            {
                bool 
                    multA = Binds.multX.IsPressed, 
                    multB = Binds.multY.IsPressed,
                    multC = Binds.multZ.IsPressed;

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
            private readonly PropertyBlock pBlock;
            private readonly ITerminalProperty<Color> property;
            private readonly string name;
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
                this.name = name;
                property = prop;
                name = GetTooltipName(prop);

                incr0 = Utilities.Clamp(maxValue / cfg.colorDiv, 1, maxValue);
                incrC = incr0 * cfg.colorMult.Z; // x64
                incrB = incr0 * cfg.colorMult.Y; // x16
                incrA = incr0 * cfg.colorMult.X; // x8

                this.delta = delta;
                this.colorDisp = colorDisp;
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

            public string GetDisplay() =>
                name + colorDisp();

            public string GetName() =>
                name;

            public string GetValue() =>
                colorDisp();

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

                curr.R = (byte)Utilities.Clamp(r, minValue, maxValue);
                curr.G = (byte)Utilities.Clamp(g, minValue, maxValue);
                curr.B = (byte)Utilities.Clamp(b, minValue, maxValue);

                property.SetValue(pBlock.TBlock, curr);
            }
            
            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private int GetIncrement()
            {
                bool 
                    multA = Binds.multX.IsPressed, 
                    multB = Binds.multY.IsPressed,
                    multC = Binds.multZ.IsPressed;

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