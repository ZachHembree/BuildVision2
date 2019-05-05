using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System;
using System.Xml.Serialization;
using DarkHelmet.IO;
using DarkHelmet.UI;
using DarkHelmet.Input;
using DarkHelmet.Game;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Stores all information needed for reading/writing from the config file.
    /// </summary>
    [XmlRoot, XmlType(TypeName = "BuildVisionSettings")]
    public class BvConfig : ConfigRootBase<BvConfig>
    {
        [XmlIgnore]
        public new static BvConfig Defaults
        {
            get
            {
                return new BvConfig
                {
                    VersionID = 5,
                    general = GeneralConfig.Defaults,
                    menu = PropMenuConfig.Defaults,
                    propertyBlock = PropBlockConfig.Defaults,
                    binds = BindsConfig.Defaults
                };
            }
        }

        [XmlElement(ElementName = "GeneralSettings")]
        public GeneralConfig general;

        [XmlElement(ElementName = "GuiSettings")]
        public PropMenuConfig menu;

        [XmlElement(ElementName = "BlockPropertySettings")]
        public PropBlockConfig propertyBlock;

        [XmlElement(ElementName = "InputSettings")]
        public BindsConfig binds;

        public BvConfig() { }

        public BvConfig GetCopy()
        {
            return new BvConfig
            {
                VersionID = VersionID,
                general = general,
                menu = menu,
                propertyBlock = propertyBlock,
                binds = binds
            };
        }

        public override void Validate()
        {
            if (VersionID < 5)
                propertyBlock = PropBlockConfig.Defaults;

            if (VersionID < 4)
                menu.apiHudConfig = ApiHudConfig.Defaults;

            if (VersionID != Defaults.VersionID)
                VersionID = Defaults.VersionID;

            if (general != null)
                general.Validate();
            else
                general = GeneralConfig.Defaults;

            if (menu != null)
                menu.Validate();
            else
                menu = PropMenuConfig.Defaults;

            if (binds != null)
                binds.Validate();
            else
                binds = BindsConfig.Defaults;

            if (propertyBlock != null)
                propertyBlock.Validate();
            else
                propertyBlock = PropBlockConfig.Defaults;
        }
    }

    public class GeneralConfig : ConfigBase<GeneralConfig>
    {
        [XmlIgnore]
        public new static GeneralConfig Defaults
        {
            get
            {
                return new GeneralConfig
                {
                    forceFallbackHud = false,
                    closeIfNotInView = true,
                    canOpenIfHolding = true
                };
            }
        }

        [XmlElement(ElementName = "ForceFallbackHud")]
        public bool forceFallbackHud;

        [XmlElement(ElementName = "CloseIfTargetNotInView")]
        public bool closeIfNotInView;

        [XmlElement(ElementName = "CanOpenIfHandsNotEmpty")]
        public bool canOpenIfHolding;

        public override void Validate()
        { }
    }

    public class PropMenuConfig : ConfigBase<PropMenuConfig>
    {
        [XmlIgnore]
        public new static PropMenuConfig Defaults
        {
            get
            {
                return new PropMenuConfig
                {
                    apiHudConfig = ApiHudConfig.Defaults,
                    fallbackHudConfig = NotifHudConfig.Defaults
                };
            }
        }

        [XmlElement(ElementName = "ApiHudSettings")]
        public ApiHudConfig apiHudConfig;

        [XmlElement(ElementName = "FallbackHudSettings")]
        public NotifHudConfig fallbackHudConfig;

        public override void Validate()
        {
            if (apiHudConfig != null)
                apiHudConfig.Validate();
            else
                apiHudConfig = ApiHudConfig.Defaults;

            if (fallbackHudConfig != null)
                fallbackHudConfig.Validate();
            else
                fallbackHudConfig = NotifHudConfig.Defaults;
        }
    }

    /// <summary>
    /// Stores config information for the Text HUD API based menu
    /// </summary>
    public class ApiHudConfig : ConfigBase<ApiHudConfig>
    {
        [XmlIgnore]
        public new static ApiHudConfig Defaults
        {
            get
            {
                return new ApiHudConfig
                {
                    hudScale = 1f,
                    maxVisible = 11,
                    hideIfNotVis = true,
                    clampHudPos = true,
                    forceToCenter = false,
                    colors = Colors.Defaults
                };
            }
        }

        [XmlElement(ElementName = "HudScale")]
        public float hudScale;

        [XmlElement(ElementName = "MaxVisibleItems")]
        public int maxVisible;

        [XmlElement(ElementName = "HideHudIfOutOfView")]
        public bool hideIfNotVis;

        [XmlElement(ElementName = "ClampHudToScreenEdges")]
        public bool clampHudPos;

        [XmlElement(ElementName = "LockHudToScreenCenter")]
        public bool forceToCenter;

        [XmlElement(ElementName = "ColorsRGB")]
        public Colors colors;

        public class Colors : ConfigBase<Colors>
        {
            [XmlIgnore]
            public new static Colors Defaults
            {
                get
                {
                    return new Colors
                    {
                        bodyText = "210,235,245",
                        headerText = "210,235,245",
                        blockIncText = "200,15,15",
                        highlightText = "200,170,0",
                        selectedText = "30,200,30",
                        backgroundColor = new Color(60, 65, 70, 190),
                        selectionBoxColor = new Color(41, 54, 62, 255),
                        headerColor = new Color(54, 66, 76, 240)
                    };
                }
            }

            [XmlIgnore]
            public Color backgroundColor, selectionBoxColor, headerColor;

            [XmlElement(ElementName = "BodyText")]
            public string bodyText;

            [XmlElement(ElementName = "HeaderText")]
            public string headerText;

            [XmlElement(ElementName = "BlockIncompleteText")]
            public string blockIncText;

            [XmlElement(ElementName = "HighlightText")]
            public string highlightText;

            [XmlElement(ElementName = "SelectedText")]
            public string selectedText;

            [XmlElement(ElementName = "ListBg")]
            public string backgroundColorData;

            [XmlElement(ElementName = "SelectionBg")]
            public string selectionBoxColorData;

            [XmlElement(ElementName = "HeaderBg")]
            public string headerColorData;

            /// <summary>
            /// Checks any if fields have invalid values and resets them to the default if necessary.
            /// </summary>
            public override void Validate()
            {
                if (bodyText == null)
                    bodyText = Defaults.bodyText;

                if (blockIncText == null)
                    blockIncText = Defaults.blockIncText;

                if (highlightText == null)
                    highlightText = Defaults.highlightText;

                if (selectedText == null)
                    selectedText = Defaults.selectedText;

                if (backgroundColorData == null || !Utilities.TryParseColor(backgroundColorData, out backgroundColor))
                {
                    backgroundColor = Defaults.backgroundColor;
                    backgroundColorData = Utilities.GetColorString(backgroundColor);
                }

                if (selectionBoxColorData == null || !Utilities.TryParseColor(selectionBoxColorData, out selectionBoxColor))
                {
                    selectionBoxColor = Defaults.selectionBoxColor;
                    selectionBoxColorData = Utilities.GetColorString(selectionBoxColor);
                }

                if (headerColorData == null || !Utilities.TryParseColor(headerColorData, out headerColor))
                {
                    headerColor = Defaults.headerColor;
                    headerColorData = Utilities.GetColorString(headerColor);
                }
            }
        }

        public ApiHudConfig() { }

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            ApiHudConfig defaults = Defaults;

            if (maxVisible == default(int))
                maxVisible = defaults.maxVisible;

            if (hudScale == default(float))
                hudScale = defaults.hudScale;

            if (colors != null)
                colors.Validate();
            else
                colors = defaults.colors;
        }
    }

    /// <summary>
    /// Stores config information for the built in Notifications based menu
    /// </summary>
    public class NotifHudConfig : ConfigBase<NotifHudConfig>
    {
        [XmlIgnore]
        public new static NotifHudConfig Defaults
        {
            get
            {
                return new NotifHudConfig
                {
                    maxVisible = 8
                };
            }
        }

        [XmlElement(ElementName = "MaxVisibleItems")]
        public int maxVisible;

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            if (maxVisible == default(int))
                maxVisible = Defaults.maxVisible;
        }
    }

    /// <summary>
    /// Stores configuration of scrollable data for serializatin.
    /// </summary>
    public class PropBlockConfig : ConfigBase<PropBlockConfig>
    {
        [XmlIgnore]
        public new static PropBlockConfig Defaults
        {
            get
            {
                return new PropBlockConfig
                {
                    floatDiv = 100.0,
                    floatMult = new Vector3(.1f, 5f, 10f),
                    colorMult = new Vector3I(8, 16, 64)
                };
            }
        }

        [XmlElement(ElementName = "FloatIncrementDivisor")]
        public double floatDiv;

        [XmlElement(ElementName = "FloatPropertyMultipliers")]
        public Vector3 floatMult;

        [XmlElement(ElementName = "ColorPropertyMultipliers")]
        public Vector3I colorMult;

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
}