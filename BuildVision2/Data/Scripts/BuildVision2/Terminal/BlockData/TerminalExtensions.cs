using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.Game.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public enum FloatValueUnits : int
    {
        None = 0,

        /// <summary>
        /// meters (m)
        /// </summary>
        Distance = 1,

        /// <summary>
        /// meters per second (m/s)
        /// </summary>
        Speed = 2,

        /// <summary>
        /// meters per second² (m/s²)
        /// </summary>
        Acceleration = 3,

        /// <summary>
        /// Newtons (N)
        /// </summary>
        Force = 4,

        /// <summary>
        /// Newton-meters (Nm)
        /// </summary>
        Torque = 5,

        /// <summary>
        /// seconds (s)
        /// </summary>
        Time = 6,

        /// <summary>
        /// °
        /// </summary>
        Angle = 7,

        /// <summary>
        /// RPM
        /// </summary>
        RevolutionsPerMinute = 8,

        /// <summary>
        /// %
        /// </summary>
        Percentage = 9,

        /// <summary>
        /// none
        /// </summary>
        Ratio = 10
    }

    public static class TerminalExtensions
    {
        public static readonly IReadOnlyDictionary<string, FloatValueUnits> FloatPropertySubtypes = new Dictionary<string, FloatValueUnits> 
        {
            { "Rotor_LowerLimit", FloatValueUnits.Angle },
            { "Rotor_UpperLimit", FloatValueUnits.Angle },
            { "Suspension_MaxSteerAngle", FloatValueUnits.Angle },

            { "Rotor_Velocity", FloatValueUnits.Speed },
            { "Piston_Velocity", FloatValueUnits.Speed },
            { "Suspension_Speed Limit", FloatValueUnits.Speed },

            { "GravityGen_Gravity", FloatValueUnits.Acceleration },

            { "Beacon_Radius", FloatValueUnits.Distance },
            { "Turret_Range", FloatValueUnits.Distance },
            { "Inventory_AutoDeployHeight", FloatValueUnits.Distance },
            { "GravityGen_Radius", FloatValueUnits.Distance },
            { "GravityGen_Width", FloatValueUnits.Distance },
            { "GravityGen_Height", FloatValueUnits.Distance },
            { "GravityGen_Depth", FloatValueUnits.Distance },
            { "Rotor_Displacement", FloatValueUnits.Distance },
            { "Light_Radius", FloatValueUnits.Distance },
            { "Piston_UpperLimit", FloatValueUnits.Distance },
            { "Piston_LowerLimit", FloatValueUnits.Distance },
            { "Suspension_Height", FloatValueUnits.Distance },
            { "OreDetector_Range", FloatValueUnits.Distance },
            { "RadioAntenna_Radius", FloatValueUnits.Distance },

            { "Rotor_Torque", FloatValueUnits.Torque },
            { "Rotor_BrakingTorque", FloatValueUnits.Torque },

            { "Piston_MaxImpulseAxis", FloatValueUnits.Force },
            { "Piston_MaxImpulseNonAxis", FloatValueUnits.Force },
            { "Thruster_Override", FloatValueUnits.Force },

            { "Connector_Strength", FloatValueUnits.Percentage },
            { "Suspension_Power", FloatValueUnits.Percentage },
            { "Suspension_Strength", FloatValueUnits.Percentage },
            { "Suspension_Friction", FloatValueUnits.Percentage },
            { "JumpDrive_JumpDistance", FloatValueUnits.Percentage },
            { "Light_Blink Lenght", FloatValueUnits.Percentage },
            { "Light_Blink Offset", FloatValueUnits.Percentage },

            { "Suspension_Propulsion override", FloatValueUnits.Ratio },
            { "Suspension_Steer override", FloatValueUnits.Ratio },
            { "Gyroscope_Power", FloatValueUnits.Ratio },

            { "Gyroscope_Yaw", FloatValueUnits.RevolutionsPerMinute },
            { "Gyroscope_Pitch", FloatValueUnits.RevolutionsPerMinute },
            { "Gyroscope_Roll", FloatValueUnits.RevolutionsPerMinute },

            { "Warhead_DetonationTime", FloatValueUnits.Time },
            { "Connector_AutoUnlockTime", FloatValueUnits.Time },
            { "Light_Blink Interval", FloatValueUnits.Time },
        };

        public static readonly IReadOnlyDictionary<FloatValueUnits, string> FloatUnitPostfixes = new Dictionary<FloatValueUnits, string>
        {
            { FloatValueUnits.None, "" },
            { FloatValueUnits.Distance, "m" },
            { FloatValueUnits.Speed, " m/s" },
            { FloatValueUnits.Acceleration, " m/s²" },
            { FloatValueUnits.Force, " N" },
            { FloatValueUnits.Torque, " Nm" },
            { FloatValueUnits.Time, "s" },
            { FloatValueUnits.Angle, "°" },
            { FloatValueUnits.RevolutionsPerMinute, " RPM" },
            { FloatValueUnits.Percentage, "%" },
            { FloatValueUnits.Ratio, "%" },
        };

        public static string GetDistanceString(float meters)
        {
            string postfix = "m";

            if (meters > 1000f)
            {
                meters /= 1000f;
                postfix = "km";
            }

            return $"{meters.ToString("G6")} {postfix}";
        }

        public static string GetPowerDisplay(float value)
        {
            float scale;
            string suffix;
            GetPowerScale(value, out scale, out suffix);

            return $"{(value * scale).ToString("G4")} {suffix}";
        }

        /// <summary>
        /// Attempts to find the most appropriate scale for the given power value (GW/MW/KW/W).
        /// </summary>
        public static void GetPowerScale(float megawatts, out float scale, out string suffix)
        {
            if (megawatts >= 1000f)
            {
                scale = .001f;
                suffix = "GW";
            }
            else if (megawatts >= 1f)
            {
                scale = 1f;
                suffix = "MW";
            }
            else if (megawatts >= .001f)
            {
                scale = 1000f;
                suffix = "KW";
            }
            else
            {
                scale = 1000000f;
                suffix = "W";
            }
        }

        public static bool IsLargeGrid(this IMyCubeBlock block) =>
            block.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large;

        /// <summary>
        /// Checks whether or not the Enabled and Visible delegates are defined and whether
        /// invoking those delegates will throw an exception.
        /// </summary>
        public static bool CanUseControl(this IMyTerminalControl control, IMyTerminalBlock tBlock)
        {
            try
            {
                if (control.Enabled != null && control.Visible != null)
                {
                    control.Enabled(tBlock);
                    control.Visible(tBlock);

                    return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Returns true if it can retrieve the current value without throwing an exception.
        /// </summary>
        public static bool CanAccessValue<TValue>(this ITerminalProperty<TValue> terminalValue, IMyTerminalBlock tBlock)
        {
            try
            {
                terminalValue.GetValue(tBlock);
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Returns true if it can retrieve the current value without throwing an exception.
        /// </summary>
        public static bool CanAccessValue<TValue>(this IMyTerminalValueControl<TValue> terminalValue, IMyTerminalBlock tBlock)
        {
            if (terminalValue.Getter != null && terminalValue.Setter != null)
            {
                try
                {
                    terminalValue.Getter(tBlock);
                    return true;
                }
                catch { }
            }

            return false;
        }

        /// <summary>
        /// Returns true if it can retrieve the current value without throwing an exception.
        /// </summary>
        public static bool CanAccessValue(this IMyTerminalControlCombobox comboBox, IMyTerminalBlock tBlock)
        {
            if (CanAccessValue(comboBox as IMyTerminalValueControl<long>, tBlock) && comboBox.ComboBoxContent != null)
            {
                try
                {
                    comboBox.ComboBoxContent(new List<MyTerminalControlComboBoxItem>());
                    return true;
                }
                catch { }
            }

            return false;
        }
    }
}