using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System;
using VRage.ModAPI;
using VRage.Game.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public static class TerminalUtilities
    {
        public static string GetForceDisplay(float newtons)
        {
            string suffix = "N";
            
            if (newtons > 1E9f)
            {
                newtons /= 1E9f;
                suffix = "GN";
            }
            else if (newtons > 1E6f)
            {
                newtons /= 1E6f;
                suffix = "MN";
            }
            else if (newtons > 1E3f)
            {
                newtons /= 1E3f;
                suffix = "kN";
            }

            return $"{Math.Round(newtons, 4):G6} {suffix}";
        }

        public static string GetDistanceDisplay(float meters)
        {
            string suffix = "m";

            if (meters > 1E3f)
            {
                meters /= 1E3f;
                suffix = "km";
            }

            return $"{Math.Round(meters, 4):G6} {suffix}";
        }

        public static string GetPowerDisplay(float megawatts)
        {
            float scale;
            string suffix;
            GetPowerScale(megawatts, out scale, out suffix);

            return $"{(megawatts * scale):G4} {suffix}";
        }

        /// <summary>
        /// Attempts to find the most appropriate scale for the given power value (GW/MW/KW/W).
        /// </summary>
        public static void GetPowerScale(float megawatts, out float scale, out string suffix)
        {
            if (megawatts >= 1E3f)
            {
                scale = 1E-3f;
                suffix = "GW";
            }
            else if (megawatts >= 1f)
            {
                scale = 1f;
                suffix = "MW";
            }
            else if (megawatts >= 1E-3f)
            {
                scale = 1E3f;
                suffix = "kW";
            }
            else
            {
                scale = 1E6f;
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