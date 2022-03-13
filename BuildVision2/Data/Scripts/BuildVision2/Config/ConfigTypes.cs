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
        public static bool WasConfigOld { get; private set; }
        private const int vID = 11;

        [XmlElement(ElementName = "TargetingSettings")]
        public TargetingConfig targeting;

        [XmlElement(ElementName = "UISettings")]
        public UIConfig genUI;

        [XmlElement(ElementName = "BlockPropertySettings")]
        public PropBlockConfig block;

        [XmlElement(ElementName = "InputSettings")]
        public BindsConfig binds;

        protected override BvConfig GetDefaults()
        {
            return new BvConfig
            {
                VersionID = vID,
                targeting = TargetingConfig.Defaults,
                genUI = UIConfig.Defaults,
                block = PropBlockConfig.Defaults,
                binds = BindsConfig.Defaults
            };
        }

        public override void Validate()
        {
            if (VersionID != vID)
            {
                VersionID = vID;
                targeting = TargetingConfig.Defaults;
                genUI = UIConfig.Defaults;
                block = PropBlockConfig.Defaults;
                binds = BindsConfig.Defaults;
                WasConfigOld = true;
            }
            else
            {
                if (targeting != null)
                    targeting.Validate();
                else
                    targeting = TargetingConfig.Defaults;

                if (genUI != null)
                    genUI.Validate();
                else
                    genUI = UIConfig.Defaults;

                if (binds != null)
                    binds.Validate();
                else
                    binds = BindsConfig.Defaults;

                if (block != null)
                    block.Validate();
                else
                    block = PropBlockConfig.Defaults;

                if (genUI.legacyModeEnabled)
                    targeting.canOpenIfPlacing = true;
            }
        }
    }

    public class TargetingConfig : Config<TargetingConfig>
    {
        [XmlElement(ElementName = "EnablePeek")]
        public bool enablePeek;

        [XmlElement(ElementName = "CloseIfTargetNotInView")]
        public bool closeIfNotInView;

        [XmlElement(ElementName = "CanOpenIfPlacing")]
        public bool canOpenIfPlacing;

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
                canOpenIfPlacing = false,
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

    /// <summary>
    /// Stores config information for the GUI
    /// </summary>
    public class UIConfig : Config<UIConfig>
    {
        [XmlElement(ElementName = "EnableLegacyMode")]
        public bool legacyModeEnabled;

        [XmlElement(ElementName = "EnableResolutionScaling")]
        public bool resolutionScaling;

        [XmlElement(ElementName = "CursorSensitivity")]
        public float cursorSensitivity;

        [XmlElement(ElementName = "HudScale")]
        public float hudScale;

        [XmlElement(ElementName = "HudOpacity")]
        public float hudOpacity;

        [XmlElement(ElementName = "ListMaxVisibleItems")]
        public int listMaxVisible;

        [XmlElement(ElementName = "WheelMaxVisibleItems")]
        public int wheelMaxVisible;

        [XmlElement(ElementName = "ClampHudToScreenEdges")]
        public bool clampHudPos;

        [XmlElement(ElementName = "UseCustomHudPosition")]
        public bool useCustomPos;

        [XmlElement(ElementName = "HudPosition")]
        public Vector2 hudPos;

        protected override UIConfig GetDefaults()
        {
            return new UIConfig
            {
                legacyModeEnabled = false,
                resolutionScaling = true,
                cursorSensitivity = .8f,
                hudScale = 1f,
                hudOpacity = 0.8f,
                listMaxVisible = 14,
                wheelMaxVisible = 16,
                clampHudPos = true,
                useCustomPos = false,
                hudPos = new Vector2(-0.4823f, 0.4676f)
            };
        }

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            cursorSensitivity = MathHelper.Clamp(cursorSensitivity, .3f, 2f);
            listMaxVisible = MathHelper.Clamp(listMaxVisible, 6, 40);
            wheelMaxVisible = MathHelper.Clamp(wheelMaxVisible, 10, 30);

            if (hudScale == default(float))
                hudScale = Defaults.hudScale;

            if (hudOpacity < 0f || hudOpacity > 1f)
                hudOpacity = Defaults.hudOpacity;

            hudPos = Vector2.Clamp(hudPos, -.49f * Vector2.One, .49f * Vector2.One);
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
        public static BindDefinition[] DefaultModifiers => defaultModifiers.Clone() as BindDefinition[];

        public static BindDefinition[] DefaultMain => defaultMain.Clone() as BindDefinition[];

        public static BindDefinition[] DefaultSecondary => defaultSecondary.Clone() as BindDefinition[];

        public static BindDefinition[] DefaultDupe => defaultDupe.Clone() as BindDefinition[];

        public static BindDefinition[] DefaultLegacyMain => defaultLegacyMain.Clone() as BindDefinition[];

        public static BindDefinition[] DefaultLegacySecondary => defaultLegacySecondary.Clone() as BindDefinition[];

        public static BindDefinition[] DefaultLegacyDupe => defaultLegacyDupe.Clone() as BindDefinition[];

        private static readonly BindDefinition[]
            defaultModifiers = new BindGroupInitializer
            {
                { "MultX/Mouse", MyKeys.Control },
                { "MultY", MyKeys.Shift },
                { "MultZ", MyKeys.Control, MyKeys.Shift },

            }.GetBindDefinitions(),
            defaultMain = new BindGroupInitializer
            {
                { "OpenWheel", MyKeys.Control, RichHudControls.MousewheelUp },
                { "OpenList", MyKeys.Control, RichHudControls.MousewheelDown },
                { "LegacyClose" },

            }.GetBindDefinitions(),
            defaultSecondary = new BindGroupInitializer
            {
                { "Select/Confirm", MyKeys.LeftButton },
                { "Cancel/Back", MyKeys.RightButton },
                { "ScrollUp", RichHudControls.MousewheelUp },
                { "ScrollDown", RichHudControls.MousewheelDown },
                { "LegacyOpen", MyKeys.Control, MyKeys.MiddleButton },

            }.GetBindDefinitions(),
            defaultDupe = new BindGroupInitializer
            {
                { "StartDupe", MyKeys.Control, MyKeys.Alt, RichHudControls.MousewheelUp },
                { "StopDupe", MyKeys.Control, MyKeys.Alt, RichHudControls.MousewheelDown },
                { "ToggleDupe", MyKeys.Home },
                { "SelectAll", MyKeys.Insert },
                { "CopySelection", MyKeys.PageUp },
                { "PasteProperties", MyKeys.PageDown },
                { "UndoPaste", MyKeys.Delete },

            }.GetBindDefinitions(),
            defaultLegacyMain = new BindGroupInitializer
            {
                { "OpenWheel" },
                { "OpenList" },
                { "LegacyClose", MyKeys.Shift, MyKeys.MiddleButton },

            }.GetBindDefinitions(),
            defaultLegacySecondary = new BindGroupInitializer
            {
                { "Select/Confirm", MyKeys.MiddleButton },
                { "Cancel/Back" },
                { "ScrollUp", RichHudControls.MousewheelUp },
                { "ScrollDown", RichHudControls.MousewheelDown },
                { "LegacyOpen", MyKeys.Control, MyKeys.MiddleButton },

            }.GetBindDefinitions(),
            defaultLegacyDupe = new BindGroupInitializer
            {
                { "StartDupe" },
                { "StopDupe" },
                { "ToggleDupe", MyKeys.Home },
                { "SelectAll", MyKeys.Insert },
                { "CopySelection", MyKeys.PageUp },
                { "PasteProperties", MyKeys.PageDown },
                { "UndoPaste", MyKeys.Delete },

            }.GetBindDefinitions();

        [XmlArray("ModifierGroup")]
        public BindDefinition[] modifierGroup;

        [XmlArray("MainGroup")]
        public BindDefinition[] mainGroup;

        [XmlArray("SecondaryGroup")]
        public BindDefinition[] secondaryGroup;

        [XmlArray("DupeGroup")]
        public BindDefinition[] dupeGroup;

        protected override BindsConfig GetDefaults()
        {
            return new BindsConfig
            {
                modifierGroup = DefaultModifiers,
                mainGroup = DefaultMain,
                secondaryGroup = DefaultSecondary,
                dupeGroup = DefaultDupe
            };
        }

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            if (modifierGroup == null)
                modifierGroup = DefaultModifiers;

            if (mainGroup == null)
                mainGroup = DefaultMain;

            if (secondaryGroup == null)
                secondaryGroup = DefaultSecondary;

            if (dupeGroup == null)
                dupeGroup = DefaultSecondary;
        }
    }
}