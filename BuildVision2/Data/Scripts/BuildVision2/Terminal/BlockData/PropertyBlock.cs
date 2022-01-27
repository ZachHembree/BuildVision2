using RichHudFramework;
using RichHudFramework.Internal;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using VRage.ModAPI;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Block property data used by the menu
    /// </summary>
    public partial class PropertyBlock : SuperBlock
    {
        public static PropBlockConfig Cfg { get { return BvConfig.Current.block; } set { BvConfig.Current.block = value; } }

        /// <summary>
        /// Read-only collection of block members
        /// </summary>
        public IReadOnlyList<IBlockMember> BlockMembers 
        {
            get 
            {
                if (blockMembers == null || blockMembers.Count == 0)
                    GenerateProperties();

                return blockMembers;
            }
        }

        /// <summary>
        /// Total number of block members currently enabled and visible
        /// </summary>
        public int EnabledMembers => GetEnabledElementCount();

        /// <summary>
        /// The difference between the center of the bounding box and the position reported by
        /// GetPosition().
        /// </summary>
        public Vector3D ModelOffset { get; private set; }

        private readonly List<BlockMemberBase> blockMembers;
        private readonly List<BvTerminalPropertyBase> blockProperties;

        private readonly BvPropPool<BlockAction> blockActionPool;
        private readonly BvPropPool<BoolProperty> boolPropPool;
        private readonly BvPropPool<ColorProperty> colorPropPool;
        private readonly BvPropPool<ComboBoxProperty> comboPropPool;
        private readonly BvPropPool<FloatProperty> floatPropPool;
        private readonly BvPropPool<TextProperty> textPropPool;
        private readonly StringBuilder nameBuilder;
        private readonly List<MyTerminalControlComboBoxItem> comboItemBuffer;

        public PropertyBlock()
        {
            blockActionPool = new BvPropPool<BlockAction>();
            boolPropPool = new BvPropPool<BoolProperty>();
            colorPropPool = new BvPropPool<ColorProperty>();
            comboPropPool = new BvPropPool<ComboBoxProperty>();
            floatPropPool = new BvPropPool<FloatProperty>();
            textPropPool = new BvPropPool<TextProperty>();
            nameBuilder = new StringBuilder();

            blockMembers = new List<BlockMemberBase>();
            blockProperties = new List<BvTerminalPropertyBase>();
            comboItemBuffer = new List<MyTerminalControlComboBoxItem>();
        }

        public override void SetBlock(TerminalGrid grid, IMyTerminalBlock tBlock)
        {
            base.SetBlock(grid, tBlock);
            ModelOffset = tBlock.WorldAABB.Center - TBlock.GetPosition();
        }

        public override void Reset()
        {
            base.Reset();

            for (int i = 0; i < blockMembers.Count; i++)
                blockMembers[i].Return();

            blockMembers.Clear();
            blockProperties.Clear();
            ModelOffset = Vector3D.Zero;
        }

        private void GenerateProperties()
        {
            blockMembers.Clear();
            blockProperties.Clear();

            GetScrollableProps();
            GetScrollableActions();
        }

        public int GetEnabledElementCount()
        {
            int count = 0;

            if (blockMembers == null)
                GenerateProperties();

            foreach (IBlockMember member in blockMembers)
                if (member.Enabled)
                    count++;

            return count;
        }

        /// <summary>
        /// Applies property settings from block data and returns the number of properties successfully updated.
        /// </summary>
        public int ImportSettings(BlockData src)
        {
            int importCount = 0;

            if (blockMembers.Count == 0)
                GenerateProperties();

            foreach (PropertyData propData in src.terminalProperties)
            {
                BvTerminalPropertyBase prop = blockProperties.Find(x => x.PropName.IsTextEqual(propData.name));

                if (prop != null)
                {
                    if (prop.TryImportData(propData))
                        importCount++;
                }
            }

            return importCount;
        }

        /// <summary>
        /// Exports block terminal settings as a serializable <see cref="BlockData"/>
        /// </summary>
        public BlockData ExportSettings()
        {
            if (blockProperties.Count == 0)
                GenerateProperties();

            var propData = new List<PropertyData>(blockProperties.Count);

            for (int n = 0; n < blockProperties.Count; n++)
                propData.Add(blockProperties[n].GetPropertyData());

            return new BlockData(TypeID, propData);
        }

        /// <summary>
        /// Retrieves a Block Property's Terminal Name.
        /// </summary>
        private static void GetTooltipName(ITerminalProperty prop, StringBuilder dst)
        {
            dst.Clear();

            if (prop is IMyTerminalControlTitleTooltip)
            {
                var tooltip = prop as IMyTerminalControlTitleTooltip;
                int trailingCharacters = 0;
                StringBuilder name = MyTexts.Get(tooltip.Title);

                for (int n = name.Length - 1; n >= 0; n--)
                {
                    if ((name[n] >= '0' && name[n] <= '9') || name[n] >= 'A')
                        break;
                    else
                        trailingCharacters++;
                }

                dst.EnsureCapacity(name.Length - trailingCharacters);

                for (int n = 0; n < (name.Length - trailingCharacters); n++)
                {
                    if (name[n] >= ' ')
                        dst.Append(name[n]);
                }
            }
        }

        /// <summary>
        /// Filters out any any special characters from a given string.
        /// </summary>
        private static void CleanText(StringBuilder src, StringBuilder dst)
        {
            if (src != null)
            {
                dst.Clear();
                dst.EnsureCapacity(src.Length);

                for (int n = 0; n < src.Length; n++)
                {
                    if (src[n] >= ' ')
                        dst.Append(src[n]);
                }
            }
        }

        /// <summary>
        /// Retrieves all block ITerminalProperty values.
        /// </summary>
        private void GetScrollableProps()
        {
            List<ITerminalProperty> properties = new List<ITerminalProperty>(12);
            TextProperty argProperty = null;
            TBlock.GetProperties(properties);

            foreach (ITerminalProperty prop in properties)
            {
                var control = prop as IMyTerminalControl;

                if (control != null && control.CanUseControl(TBlock))
                {
                    GetTooltipName(prop, nameBuilder);
                    
                    if (nameBuilder.Length > 0)
                    {                        
                        if (prop is ITerminalProperty<StringBuilder>)
                        {
                            var textProp = prop as ITerminalProperty<StringBuilder>;

                            if (textProp.CanAccessValue(TBlock))
                            {
                                if (prop.Id == "ConsoleCommand")
                                    argProperty = TextProperty.GetProperty(nameBuilder, textProp, this);
                                else if (prop.Id == "Name" || prop.Id == "CustomName")
                                    blockProperties.Insert(0, TextProperty.GetProperty(nameBuilder, textProp, this));
                                else
                                    blockProperties.Add(TextProperty.GetProperty(nameBuilder, textProp, this));
                            }
                        }
                        if (prop is IMyTerminalControlCombobox)
                        {
                            var comboBox = prop as IMyTerminalControlCombobox;

                            if (comboBox.CanAccessValue(TBlock, comboItemBuffer))
                                blockProperties.Add(ComboBoxProperty.GetProperty(nameBuilder, comboBox, comboItemBuffer, this));
                        }
                        else if (prop is ITerminalProperty<bool>)
                        {
                            var boolProp = prop as ITerminalProperty<bool>;

                            if (boolProp.CanAccessValue(TBlock))
                                blockProperties.Add(BoolProperty.GetProperty(nameBuilder, boolProp, this));
                        }
                        else if (prop is ITerminalProperty<float>)
                        {
                            var floatProp = prop as ITerminalProperty<float>;

                            if (floatProp.CanAccessValue(TBlock))
                                blockProperties.Add(FloatProperty.GetProperty(nameBuilder, floatProp, this));
                        }
                        else if (prop is ITerminalProperty<Color>)
                        {
                            var colorProp = prop as ITerminalProperty<Color>;

                            if (colorProp.CanAccessValue(TBlock))
                                blockProperties.Add(ColorProperty.GetProperty(nameBuilder, colorProp, this));
                        }
                    }
                }
            }

            if (argProperty != null)
                blockProperties.Add(argProperty);

            blockMembers.AddRange(blockProperties);
            comboItemBuffer.Clear();
        }

        /// <summary>
        /// Retrieves a set of custom block actions.
        /// </summary>
        private void GetScrollableActions()
        {
            if (SubtypeId.UsesSubtype(TBlockSubtypes.MechanicalConnection))
                BlockAction.GetMechActions(this, blockMembers);

            if (SubtypeId.UsesSubtype(TBlockSubtypes.Door))
                BlockAction.GetDoorActions(this, blockMembers);

            if (SubtypeId.UsesSubtype(TBlockSubtypes.Warhead))
                BlockAction.GetWarheadActions(this, blockMembers);

            if (SubtypeId.UsesSubtype(TBlockSubtypes.LandingGear))
                BlockAction.GetGearActions(this, blockMembers);

            if (SubtypeId.UsesSubtype(TBlockSubtypes.Connector))
                BlockAction.GetConnectorActions(this, blockMembers);

            if (SubtypeId.UsesSubtype(TBlockSubtypes.Programmable))
                BlockAction.GetProgrammableBlockActions(this, blockMembers);

            if (SubtypeId.UsesSubtype(TBlockSubtypes.Timer))
                BlockAction.GetTimerActions(this, blockMembers);
        }
    }
}