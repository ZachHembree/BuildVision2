using RichHudFramework;
using RichHudFramework.Internal;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using System;
using VRage;
using VRageMath;
using VRage.ModAPI;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Collection of wrapper objects used to interop with SE terminal system
    /// </summary>
    public partial class PropertyBlock : SuperBlock, IPropertyBlock
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
        public int EnabledMemberCount => GetEnabledElementCount();

        /// <summary>
        /// The difference between the center of the bounding box and the position reported by
        /// GetPosition().
        /// </summary>
        public Vector3D ModelOffset { get; private set; }

        /// <summary>
        /// Controls serialization/deserialization of terminal block properties for duplication
        /// </summary>
        public IReadOnlyBlockPropertyDuplicator Duplicator { get; }

        /// <summary>
        /// Controls prioritization of block properties
        /// </summary>
        public IReadOnlyBlockPropertyPrioritizer Prioritizer { get; }

        private readonly List<BlockMemberBase> blockMembers;
        private readonly List<BlockPropertyBase> blockProperties;

        private readonly BvPropPool<BlockAction> blockActionPool;
        private readonly BvPropPool<BoolProperty> boolPropPool;
        private readonly BvPropPool<ColorProperty> colorPropPool;
        private readonly BvPropPool<ComboBoxProperty> comboPropPool;
        private readonly BvPropPool<FloatProperty> floatPropPool;
        private readonly BvPropPool<TextProperty> textPropPool;
        private readonly StringBuilder nameBuilder;
        private readonly List<MyTerminalControlComboBoxItem> comboItemBuffer;

        private readonly BlockPropertyDuplicator duplicator;
        private readonly BlockPropertyPrioritizer prioritizer;

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
            blockProperties = new List<BlockPropertyBase>();
            comboItemBuffer = new List<MyTerminalControlComboBoxItem>();

            duplicator = new BlockPropertyDuplicator();
            prioritizer = new BlockPropertyPrioritizer();

            Duplicator = duplicator;
            Prioritizer = prioritizer;
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

            duplicator.Reset();
            prioritizer.Reset();
        }

        public override void Update()
        {
            prioritizer.UpdatePrioritizedMembers(BvConfig.Current.genUI.wheelMaxVisible);
        }

        private void GenerateProperties()
        {
            blockMembers.Clear();
            blockProperties.Clear();

            GetScrollableProps();
            GetScrollableActions();

            duplicator.SetBlockMembers(this);
            prioritizer.SetBlockMembers(TBlock.GetType(), blockMembers);
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

            foreach (PropertyData propData in src.propertyList)
            {
                BlockPropertyBase prop = blockProperties.Find(x => x.PropName == propData.propName);

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
        public void ExportSettings(ref BlockData blockData)
        {
            if (blockProperties.Count == 0)
                GenerateProperties();

            for (int n = 0; n < blockProperties.Count; n++)
            {
                PropertyData? data = blockProperties[n].GetPropertyData();

                if (data != null)
                    blockData.propertyList.Add(data.Value);
            }

            blockData.blockTypeID = TypeID;
        }

        /// <summary>
        /// Retrieves a Block Property's Terminal Name.
        /// </summary>
        private static void GetTooltipName(ITerminalProperty prop, StringBuilder dst)
        {
            if (prop is IMyTerminalControlTitleTooltip)
            {
                var tooltip = prop as IMyTerminalControlTitleTooltip;
                StringBuilder name = MyTexts.Get(tooltip.Title);

                // Exclude leading spaces
                int start = 0;

                for (int i = 0; i < name.Length; i++)
                {
                    if (name[i] > ' ')
                        break;

                    start++;
                }

                dst.Clear();
                dst.EnsureCapacity(name.Length - start);

                for (int n = start; n < name.Length; n++)
                {
                    char ch = name[n],
                        nextCh = name[Math.Min(n + 1, name.Length - 1)];

                    if (
                        // Exclude special chars and most punctuation
                        (ch == ' ' || ch >= 'A' || (ch >= '0' && ch <= '9'))
                        // Exclude repeating and trailing spaces
                        && !(nextCh == ' ' && ch == ' '))
                    {
                        dst.Append(ch);
                    }
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
                // Exclude leading spaces
                int start = 0;

                for (int i = 0; i < src.Length; i++)
                {
                    if (src[i] > ' ')
                        break;

                    start++;
                }

                dst.Clear();
                dst.EnsureCapacity(src.Length - start);

                for (int n = start; n < src.Length; n++)
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