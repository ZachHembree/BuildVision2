using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Xml.Serialization;
using VRage.Input;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    [XmlRoot, XmlType(TypeName = "BuildVisionSettings")]
    public class BvConfig : ConfigRoot<BvConfig>
    {
        private const int vID = 9;

        [XmlElement(ElementName = "GeneralSettings")]
        public TargetingConfig general;

        [XmlElement(ElementName = "HudConfig")]
        public HudConfig hudConfig;

        [XmlElement(ElementName = "BlockPropertySettings")]
        public PropBlockConfig block;

        [XmlElement(ElementName = "InputSettings")]
        public BindsConfig binds;

        protected override BvConfig GetDefaults()
        {
            return new BvConfig
            {
                VersionID = vID,
                general = TargetingConfig.Defaults,
                hudConfig = HudConfig.Defaults,
                block = PropBlockConfig.Defaults,
                binds = BindsConfig.Defaults
            };
        }

        public override void Validate()
        {
            if (VersionID < vID)
            {
                general = TargetingConfig.Defaults;
                hudConfig = HudConfig.Defaults;
                block = PropBlockConfig.Defaults;
                binds = BindsConfig.Defaults;
            }
            else
            {
                if (general != null)
                    general.Validate();
                else
                    general = TargetingConfig.Defaults;

                if (hudConfig != null)
                    hudConfig.Validate();
                else
                    hudConfig = HudConfig.Defaults;

                if (binds != null)
                    binds.Validate();
                else
                    binds = BindsConfig.Defaults;

                if (block != null)
                    block.Validate();
                else
                    block = PropBlockConfig.Defaults;
            }
        }
    }

    public class TargetingConfig : Config<TargetingConfig>
    {
        [XmlElement(ElementName = "EnablePeek")]
        public bool enablePeek;

        [XmlElement(ElementName = "CloseIfTargetNotInView")]
        public bool closeIfNotInView;

        [XmlElement(ElementName = "CanOpenIfHandsNotEmpty")]
        public bool canOpenIfHolding;

        [XmlElement(ElementName = "maxOpenRange")]
        public double maxOpenRange;

        [XmlElement(ElementName = "maxControlRange")]
        public double maxControlRange;

        protected override TargetingConfig GetDefaults()
        {
            return new TargetingConfig
            {
                enablePeek = true,
                closeIfNotInView = true,
                canOpenIfHolding = true,
                maxOpenRange = 10d,
                maxControlRange = 10d
            };
        }

        public override void Validate()
        {
            if (maxOpenRange == 0d)
                maxOpenRange = Defaults.maxOpenRange;
            else
                maxOpenRange = MathHelper.Clamp(maxOpenRange, 2.5d, 20d);

            if (maxControlRange == 0d)
                maxControlRange = Defaults.maxControlRange;
            else
                maxControlRange = MathHelper.Clamp(maxControlRange, maxOpenRange, 60d);
        }
    }

    [Obsolete]
    public class PropMenuConfig : Config<PropMenuConfig>
    {
        [XmlElement(ElementName = "ApiHudSettings")]
        public HudConfig hudConfig;

        protected override PropMenuConfig GetDefaults()
        {
            return new PropMenuConfig
            {
                hudConfig = HudConfig.Defaults,
            };
        }

        public override void Validate()
        {
            if (hudConfig != null)
                hudConfig.Validate();
            else
                hudConfig = HudConfig.Defaults;
        }
    }

    /// <summary>
    /// Stores config information for the Text HUD based menu
    /// </summary>
    public class HudConfig : Config<HudConfig>
    {
        [XmlElement(ElementName = "EnableResolutionScaling")]
        public bool resolutionScaling;

        [XmlElement(ElementName = "HudScale")]
        public float hudScale;

        [XmlElement(ElementName = "HudOpacity")]
        public float hudOpacity;

        [XmlElement(ElementName = "MaxVisibleItems")]
        public int maxVisible;

        [XmlElement(ElementName = "ClampHudToScreenEdges")]
        public bool clampHudPos;

        [XmlElement(ElementName = "UseCustomHudPosition")]
        public bool useCustomPos;

        [XmlElement(ElementName = "HudPosition")]
        public Vector2 hudPos;

        protected override HudConfig GetDefaults()
        {
            return new HudConfig
            {
                resolutionScaling = true,
                hudScale = 1f,
                hudOpacity = 0.9f,
                maxVisible = 14,
                clampHudPos = true,
                useCustomPos = false,
                hudPos = new Vector2(-0.483125f, 0.47f)
            };
        }

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            if (maxVisible == default(int))
                maxVisible = Defaults.maxVisible;

            if (hudScale == default(float))
                hudScale = Defaults.hudScale;

            if (hudOpacity < 0f)
                hudOpacity = Defaults.hudOpacity;
        }
    }

    /// <summary>
    /// Stores configuration of scrollable data for serializatin.
    /// </summary>
    public class PropBlockConfig : Config<PropBlockConfig>
    {
        [XmlElement(ElementName = "FloatIncrementDivisor")]
        public double floatDiv;

        [XmlElement(ElementName = "FloatPropertyMultipliers")]
        public Vector3 floatMult;

        [XmlElement(ElementName = "ColorPropertyMultipliers")]
        public Vector3I colorMult;

        protected override PropBlockConfig GetDefaults()
        {
            return new PropBlockConfig
            {
                floatDiv = 100.0,
                floatMult = new Vector3(.1f, 5f, 10f),
                colorMult = new Vector3I(8, 16, 64)
            };
        }

        /// <summary>
        /// Checks for any fields that have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            PropBlockConfig defaults = Defaults;

            if (floatDiv <= 0f)
                floatDiv = defaults.floatDiv;

            // Float multipliers
            if (floatMult.X <= 0f)
                floatMult.X = defaults.floatMult.X;

            if (floatMult.Y <= 0f)
                floatMult.Y = defaults.floatMult.Y;

            if (floatMult.Z <= 0f)
                floatMult.Z = defaults.floatMult.Z;

            // Color multipiers
            if (colorMult.X <= 0)
                colorMult.X = defaults.colorMult.X;

            if (colorMult.Y <= 0)
                colorMult.Y = defaults.colorMult.Y;

            if (colorMult.Z <= 0)
                colorMult.Z = defaults.colorMult.Z;
        }
    }

    /// <summary>
    /// Stores data for serializing the configuration of the Binds class.
    /// </summary>
    public class BindsConfig : Config<BindsConfig>
    {
        public static BindDefinition[] DefaultOpen => defaultOpen.Clone() as BindDefinition[];

        public static BindDefinition[] DefaultMain => defaultMain.Clone() as BindDefinition[];

        private static readonly BindDefinition[]
            defaultOpen = new BindGroupInitializer
            {
                { "Peek", MyKeys.Control },
                { "Open", MyKeys.Control, MyKeys.MiddleButton },
            }.GetBindDefinitions(),
            defaultMain = new BindGroupInitializer
            {
                { "Close", MyKeys.Shift, MyKeys.MiddleButton },
                { "Select", MyKeys.MiddleButton },
                { "ScrollUp", RichHudControls.MousewheelUp },
                { "ScrollDown", RichHudControls.MousewheelDown },
                { "MultX", MyKeys.Control },
                { "MultY", MyKeys.Shift },
                { "MultZ", MyKeys.Control, MyKeys.Shift },

                { "ToggleSelectMode", MyKeys.Home },
                { "SelectAll", MyKeys.Insert },
                { "CopySelection", MyKeys.PageUp },
                { "PasteProperties", MyKeys.PageDown },
                { "UndoPaste", MyKeys.Delete },
            }.GetBindDefinitions();

        [XmlArray("OpenGroup")]
        public BindDefinition[] openGroup;

        [XmlArray("MainGroup")]
        public BindDefinition[] mainGroup;

        protected override BindsConfig GetDefaults()
        {
            return new BindsConfig
            {
                openGroup = DefaultOpen,
                mainGroup = DefaultMain
            };
        }

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            if (openGroup == null)
                openGroup = DefaultOpen;

            if (mainGroup == null)
                mainGroup = DefaultMain;
        }
    }
}