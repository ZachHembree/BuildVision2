using RichHudFramework;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;
using IMyParachute = SpaceEngineers.Game.ModAPI.Ingame.IMyParachute;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Block property data used by the menu
    /// </summary>
    internal partial class PropertyBlock
    {
        public static PropBlockConfig Cfg { get { return BvConfig.Current.block; } set { BvConfig.Current.block = value; } }

        /// <summary>
        /// Associated terminal block
        /// </summary>
        public IMyTerminalBlock TBlock { get; private set; }

        /// <summary>
        /// Block type identifier. Uses IMyCubeBlock.BlockDefinition.TypeIdString.
        /// </summary>
        public string TypeID { get; }

        /// <summary>
        /// Read-only collection of block members
        /// </summary>
        public ReadOnlyCollection<IBlockMember> BlockMembers { get; }

        /// <summary>
        /// Total number of block members currently enabled and visible
        /// </summary>
        public int EnabledMembers => GetEnabledElementCount();

        /// <summary>
        /// True if the block integrity is above its breaking threshold
        /// </summary>
        public bool IsFunctional { get { return TBlock.IsFunctional; } }

        /// <summary>
        /// True if the block is functional and able to do work.
        /// </summary>
        public bool IsWorking { get { return TBlock.IsWorking; } }

        /// <summary>
        /// True if the local player has terminal access permissions
        /// </summary>
        public bool CanLocalPlayerAccess { get { return TBlock.HasLocalPlayerAccess(); } }

        public readonly Vector3D modelOffset;

        private readonly List<IBlockMember> blockMembers;
        private readonly List<BvTerminalPropertyBase> blockProperties;

        public PropertyBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;
            TypeID = tBlock.BlockDefinition.TypeIdString;

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

        public void ImportSettings(BlockData src)
        {
            foreach (PropertyData propData in src.terminalProperties)
            {
                BvTerminalPropertyBase prop = blockProperties.Find(x => (x.ID == propData.id) && (x.PropName == propData.name));

                if (prop != null)
                    prop.TryImportPropertyValue(propData);
            }
        }

        public BlockData ExportSettings()
        {
            var propData = new List<PropertyData>(blockProperties.Count);

            for (int n = 0; n < blockProperties.Count; n++)
                propData.Add(blockProperties[n].GetPropertyData());

            return new BlockData(TypeID, propData.ToArray());
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
    }
}