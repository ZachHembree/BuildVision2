using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sandbox.Definitions;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    [Flags]
    public enum TerminalPermissionStates : int
    {
        None = 0x0,
        Denied = 0x1,
        Granted = 0x2,

        GridUnowned = 0x4,
        GridUnfriendly = 0x8,
        GridFriendly = 0x10,

        BlockUnfriendly = 0x20,
        BlockFriendly = 0x40
    }

    public static class TerminalUtilities
    {
        // Maps float magnitude to metrix prefixes i.e. 10E9 => G (Giga)
        public static IReadOnlyDictionary<float, char> MetricMagPrefixTable = new Dictionary<float, char>
        {
            { 1E24f, 'Y' },
            { 1E21f, 'Z' },
            { 1E18f, 'E' },
            { 1E15f, 'P' },
            { 1E12f, 'T' },
            { 1E9f, 'G' },
            { 1E6f, 'M' },
            { 1E3f, 'k' },
            { 1E-1f, 'd' },
            { 1E-2f, 'c' },
            { 1E-3f, 'm' },
        };
        public static IReadOnlyDictionary<char, float> MetricPrefixMagTable = new Dictionary<char, float>
        {
            { 'Y', 1E24f },
            { 'Z', 1E21f },
            { 'E', 1E18f },
            { 'P', 1E15f },
            { 'T', 1E12f },
            { 'G', 1E9f },
            { 'M', 1E6f },
            { 'k', 1E3f },
            { 'd', 1E-1f },
            { 'c', 1E-2f },
            { 'm', 1E-3f },
        };
        public static ICollection<char> MetricPrefixes = new HashSet<char>
        {
            { 'Y' },
            { 'Z' },
            { 'E' },
            { 'P' },
            { 'T' },
            { 'G' },
            { 'M' },
            { 'k' },
            { 'd' },
            { 'c' },
            { 'm' },
        };
        public static IReadOnlyList<string> SpecialPrefixes = new string[]
        {
            "�", "m/s", "rpm"
        };
        public const float MetricPrefixMax = 1E24f, MetricPrefixMin = 1E-3f;

        private static Dictionary<Type, bool> ownableBlockMap;
        private static StringBuilder textBuf;

        public static void Init()
        {
            ownableBlockMap = new Dictionary<Type, bool>();
            textBuf = new StringBuilder();
        }

        public static void Close()
        {
            ownableBlockMap = null;
            textBuf = null;
        }

        /// <summary>
        /// Returns true if the block has ownership permissions
        /// </summary>
        public static bool GetIsBlockOwnable(this IMyTerminalBlock block)
        {
            bool isOwnable;

            if (!ownableBlockMap.TryGetValue(block.GetType(), out isOwnable))
            {
                IMyCubeGrid grid = block.CubeGrid;
                var def = MyDefinitionManager.Static.GetDefinition(block.BlockDefinition) as MyCubeBlockDefinition;

                // Terminal blocks with computers are ownable. If there are no bigOwners, the grid is unowned.
                isOwnable = def?.Components.Any(x => x.Definition.Id.SubtypeName == "Computer") ?? false;
                ownableBlockMap.Add(block.GetType(), isOwnable);
            }

            return isOwnable;
        }

        /// <summary>
        /// Returns true if the player can access the given terminal block. Blocks without ownership
        /// permissions require the player to have at least friendly relations with a big owner's faction.
        /// </summary>
        public static TerminalPermissionStates GetAccessPermissions(this IMyTerminalBlock block, long plyID = -1)
        {
            IMyCubeGrid grid = block.CubeGrid;
            TerminalPermissionStates accessState;

            if (plyID == -1)
                plyID = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;

            if (block.GetIsBlockOwnable())
            {
                if (block.HasPlayerAccess(plyID))
                    accessState = TerminalPermissionStates.Granted | TerminalPermissionStates.BlockFriendly;
                else
                    accessState = TerminalPermissionStates.Denied | TerminalPermissionStates.BlockUnfriendly;

                return accessState;
            }
            else
            {
                return grid.GetAccessPermissions(plyID);
            }
        }

        /// <summary>
        /// Returns grid ownership permissions for the player. Access is granted if a grid is unowned 
        /// or if at least one big owner is friendly, otherwise, the grid is considered unfriendly.
        /// </summary>
        public static TerminalPermissionStates GetAccessPermissions(this IMyCubeGrid grid, long plyID = -1)
        {
            // Ensure owners are up to date
            grid.UpdateOwnership(0, false);

            if (plyID == -1)
                plyID = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;

            TerminalPermissionStates accessState;
            List<long> bigOwners = grid.BigOwners;
            bool gridUnowned = bigOwners.Count == 0;

            if (gridUnowned)
            {
                accessState = TerminalPermissionStates.Granted | TerminalPermissionStates.GridUnowned;
            }
            else
            {
                bool gridFriendly = bigOwners.Contains(plyID);

                if (!gridFriendly)
                {
                    foreach (long owner in bigOwners)
                    {
                        IMyFaction ownerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

                        if (ownerFaction != null && (ownerFaction.IsFriendly(plyID) || ownerFaction.IsMember(plyID)))
                        {
                            gridFriendly = true;
                            break;
                        }
                    }
                }

                if (gridFriendly)
                    accessState = TerminalPermissionStates.Granted | TerminalPermissionStates.GridFriendly;
                else
                    accessState = TerminalPermissionStates.Denied | TerminalPermissionStates.GridUnfriendly;
            }

            return accessState;
        }

        /// <summary>
        /// Appends the given string to the destination stringbuilder up to a maximum length
        /// </summary>
        public static void AppendSubstringMax(this StringBuilder dst, string src, int maxLength)
        {
            textBuf.Clear();
            textBuf.Append(src);

            maxLength = Math.Max(maxLength, 0);
            dst.AppendSubstring(textBuf, 0, Math.Min(src.Length, maxLength - 3));

            if (src.Length > maxLength - 3)
                dst.Append("...");
        }

        /// <summary>
        /// Appends the given string to the destination stringbuilder up to a maximum length
        /// </summary>
        public static void AppendSubstringMax(this StringBuilder dst, StringBuilder src, int maxLength)
        {
            maxLength = Math.Max(maxLength, 0);
            dst.AppendSubstring(src, 0, Math.Min(src.Length, maxLength - 3));

            if (src.Length > maxLength - 3)
                dst.Append("...");
        }

        public static void GetForceDisplay(float newtons, StringBuilder sb)
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

            sb.AppendFormat("{0:G6}", Math.Round(newtons, 2));
            sb.Append(" ");
            sb.Append(suffix);
        }

        public static void GetDistanceDisplay(float meters, StringBuilder dst)
        {
            string suffix = "m";

            if (meters > 1E3f)
            {
                meters /= 1E3f;
                suffix = "km";
            }

            dst.AppendFormat("{0:G6}", Math.Round(meters, 2));
            dst.Append(" ");
            dst.Append(suffix);
        }

        public static void GetPowerDisplay(float megawatts, StringBuilder dst)
        {
            float scale;
            string suffix;
            GetPowerScale(megawatts / 10f, out scale, out suffix);

            dst.AppendFormat("{0:G4}", Math.Round(megawatts * scale, 2));
            dst.Append(" ");
            dst.Append(suffix);
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

        /// <summary>
        /// Indicates whether or not the cube block is a large grid block.
        /// </summary>
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
        public static bool CanAccessValue(this IMyTerminalControlCombobox comboBox, IMyTerminalBlock tBlock, List<MyTerminalControlComboBoxItem> contentBuffer)
        {
            if (CanAccessValue(comboBox, tBlock) && comboBox.ComboBoxContent != null)
            {
                try
                {
                    contentBuffer.Clear();
                    comboBox.ComboBoxContent(contentBuffer);
                    return true;
                }
                catch { }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the string builder contains the given substring
        /// </summary>
        public static bool ContainsSubstring(this StringBuilder text, int start, string substring)
        {
            if (text.Length >= substring.Length)
            {
                int count = 0;

                for (int i = start; (i - start) < substring.Length && i < text.Length; i++)
                {
                    if (text[i] == substring[i - start])
                        count++;
                }

                return count == substring.Length;
            }
            else
                return false;
        }

        public static bool IsTextEqual(this StringBuilder text, StringBuilder other)
        {
            if (text.Length == other.Length)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] != other[i])
                        return false;
                }

                return true;
            }
            else
                return false;   
        }

        public static bool IsTextEqual(this StringBuilder text, string other)
        {
            if (text != null && other != null && text.Length == other.Length)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] != other[i])
                        return false;
                }

                return true;
            }
            else
                return false;
        }

        public static void GetBeautifiedTypeID(string subtypeId, StringBuilder textBuf)
        {
            textBuf.EnsureCapacity(subtypeId.Length + 4);
            char left = 'A';

            for (int n = 0; n < subtypeId.Length; n++)
            {
                char right = subtypeId[n];
                bool rightCapital = right >= 'A' && right <= 'Z',
                    rightNumber = right >= '0' && right <= '9',
                    leftNumber = left >= '0' && left <= '9',
                    leftCapital = left >= 'A' && left <= 'Z';

                if (right == '_')
                    right = ' ';
                else if (!leftCapital && ((rightCapital && left != ' ') || (rightNumber && !leftNumber)))
                    textBuf.Append(' ');

                textBuf.Append(right);
                left = right;
            }
        }

        /// <summary>
        /// Returns true if the given character is in the range of characters used for
        /// numeric strings, 0-9, E/e, +/-, ., :
        /// </summary>
        public static bool IsNumeric(this char ch)
        {
            return (ch >= '0' && ch <= '9') || ch == 'E' || ch == 'e' || ch == '-' || ch == '+' || ch == '.' || ch == ':';
        }

        /// <summary>
        /// Returns true if the given character is alphabetical, A-z
        /// </summary>
        public static bool IsAlphabetical(this char ch)
        {
            return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
        }
    }
}