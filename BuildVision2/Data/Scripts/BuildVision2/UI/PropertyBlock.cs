using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.Game.Entities.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DoorStatus = Sandbox.ModAPI.Ingame.DoorStatus;
using ChargeMode = Sandbox.ModAPI.Ingame.ChargeMode;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;
using IMyBatteryBlock = Sandbox.ModAPI.Ingame.IMyBatteryBlock;
using IMyParachute = SpaceEngineers.Game.ModAPI.Ingame.IMyParachute;
using IMyTerminalAction = Sandbox.ModAPI.Interfaces.ITerminalAction;

namespace DarkHelmet.BuildVision2
{
    internal interface IScrollableAction
    {
        /// <summary>
        /// Retrieves the block action name and related info.
        /// </summary>
        /// <returns></returns>
        Func<string> GetName { get; }

        /// <summary>
        /// Triggers action associated with block;
        /// </summary>
        Action Action { get; }
    }
    
    internal interface IScrollableProp
    {
        /// <summary>
        /// Retrieves the block property name and related info.
        /// </summary>
        /// <returns></returns>
        string GetName();

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
    public struct PropBlockConfig
    {
        [XmlIgnore]
        public static readonly PropBlockConfig defaults = 
            new PropBlockConfig(1000.0, 256, new Vector3(10f, 25f, 100f), new Vector3I(8, 16, 64));

        [XmlElement(ElementName = "FloatIncrementDivisor")]
        public double floatDiv;

        [XmlElement(ElementName = "ColorIncrementDivisor")]
        public int colorDiv;

        [XmlElement(ElementName = "FloatPropertyMultipliers")]
        public Vector3 floatMult;

        [XmlElement(ElementName = "ColorPropertyMultipliers")]
        public Vector3I colorMult;

        public PropBlockConfig(double floatDiv, int colorDiv, Vector3D floatMult, Vector3I colorMult)
        {
            this.floatDiv = floatDiv;
            this.colorDiv = colorDiv;
            this.floatMult = floatMult;
            this.colorMult = colorMult;
        }

        /// <summary>
        /// Checks any fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public void Validate()
        {
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
        private static double floatDiv;
        private static int colorDiv;
        private static Vector3 floatMult;
        private static Vector3I colorMult;

        static PropertyBlock()
        {
            UpdateConfig(PropBlockConfig.defaults);
        }

        public PropertyBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;

            Actions = GetScrollableActions();
            Properties = GetScrollableProps();
            ScrollableCount = Actions.Count + Properties.Count;
        }
        
        /// <summary>
        /// Updates the current configuration.
        /// </summary>
        public static void UpdateConfig(PropBlockConfig cfg)
        {
            floatDiv = cfg.floatDiv;
            colorDiv = cfg.colorDiv;
            floatMult = cfg.floatMult;
            colorMult = cfg.colorMult;
        }

        /// <summary>
        /// Returns the current configuration.
        /// </summary>
        public static PropBlockConfig GetConfig()
        {
            return new PropBlockConfig
            {
                floatDiv = floatDiv,
                colorDiv = colorDiv,
                floatMult = floatMult,
                colorMult = colorMult
            };
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
            IMyMechanicalConnectionBlock mechBlock = TBlock as IMyMechanicalConnectionBlock;
            IMyDoor door = TBlock as IMyDoor;
            IMyWarhead warhead = TBlock as IMyWarhead;
            IMyLandingGear landingGear = TBlock as IMyLandingGear;
            IMyParachute parachute = TBlock as IMyParachute;
            IsMechConnection = mechBlock != null;
            //RecreateTop

            if (mechBlock != null)
                BlockAction.GetMechActions(mechBlock, actions);

            if (door != null)
                BlockAction.GetDoorActions(door, actions);

            if (warhead != null)
                BlockAction.GetWarheadActions(warhead, actions);

            if (landingGear != null)
                BlockAction.GetGearActions(landingGear, actions);

            if (parachute != null)
                BlockAction.GetChuteActions(parachute, actions);

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
                    if (prop.TypeName == "Boolean")
                        scrollables.Add(new BoolProperty(name, prop.AsBool(), this));
                    else if (prop.TypeName == "Single")
                        scrollables.Add(new FloatProperty(name, prop.AsFloat(), this));
                    else if (prop.TypeName == "Color")
                        scrollables.AddRange(ColorProperty.GetColorProperties(name, this, prop.AsColor()));
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
            public Func<string> GetName { get; private set; }
            public Action Action { get; private set; }

            public BlockAction(Func<string> GetName, Action Action)
            {
                this.GetName = GetName;
                this.Action = Action;
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyMechanicalConnectionBlock.
            /// </summary>
            public static void GetMechActions(IMyMechanicalConnectionBlock mechBlock, List<IScrollableAction> actions)
            {
                IMyPistonBase piston;
                IMyMotorStator rotor;
                
                if (mechBlock != null)
                {
                    List<IMyTerminalAction> terminalActions = new List<IMyTerminalAction>();
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
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyDoor.
            /// </summary>
            public static void GetDoorActions(IMyDoor doorBlock, List<IScrollableAction> actions)
            {
                if (doorBlock != null)
                {
                    actions.Add(new BlockAction(
                        () => "Open/Close",
                        () => doorBlock.ToggleDoor()));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyWarhead.
            /// </summary>
            public static void GetWarheadActions(IMyWarhead warhead, List<IScrollableAction> actions)
            {
                if (warhead != null)
                {
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
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyLandingGear.
            /// </summary>
            public static void GetGearActions(IMyLandingGear landingGear, List<IScrollableAction> actions)
            {
                if (landingGear != null)
                {
                    actions.Add(new BlockAction(
                        () => $"Lock/Unlock ({(landingGear.IsLocked ? "Locked" : "Unlocked")})",
                        () => landingGear.ToggleLock()));
                }
            }

            /// <summary>
            /// Gets actions for blocks implementing IMyParachute.
            /// </summary>
            public static void GetChuteActions(IMyParachute parachute, List<IScrollableAction> actions)
            {
                if (parachute != null)
                {
                    actions.Add(new BlockAction(
                        () => $"Open/Close " +
                        $"({((parachute.Status == DoorStatus.Open || parachute.Status == DoorStatus.Opening) ? "Open" : "Closed")})",
                        () => parachute.ToggleDoor()));
                }
            }
        }

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

            public string GetName() =>
                name + ": " + GetChargeModeName();

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

                index = Clamp(index, 1, 3);

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

            public string GetName() =>
                name + ": " + GetPropStateText();

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
                
                incr0 = Clamp((float)Math.Round(maxValue / floatDiv), .1f, float.PositiveInfinity); 
                incrC = incr0 * floatMult.Z; // x64
                incrB = incr0 * floatMult.Y; // x16
                incrA = incr0 * floatMult.X; // x8
            }

            public string GetName() =>
                prop.Id + ": " + prop.GetValue(pBlock.TBlock);

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

                prop.SetValue(pBlock.TBlock, (float)Math.Round(Clamp((current + delta), minValue, maxValue), 1));
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

                incr0 = Clamp(maxValue / colorDiv, 1, maxValue);
                incrC = incr0 * colorMult.Z; // x64
                incrB = incr0 * colorMult.Y; // x16
                incrA = incr0 * colorMult.X; // x8

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

            public string GetName() =>
                name + colorDisp();

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

                curr.R = (byte)Clamp(r, minValue, maxValue);
                curr.G = (byte)Clamp(g, minValue, maxValue);
                curr.B = (byte)Clamp(b, minValue, maxValue);

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

        /// <summary>
        /// Clamps a float between two values.
        /// </summary>
        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        /// <summary>
        /// Clamps an int between two values.
        /// </summary>
        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }
    }
}