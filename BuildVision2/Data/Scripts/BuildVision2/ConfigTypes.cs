using System.Xml.Serialization;
using DarkHelmet.IO;
using DarkHelmet.UI;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    [XmlRoot, XmlType(TypeName = "BuildVisionSettings")]
    public class BvConfig : ConfigRoot<BvConfig>
    {
        [XmlElement(ElementName = "GeneralSettings")]
        public GeneralConfig general;

        [XmlElement(ElementName = "GuiSettings")]
        public PropMenuConfig menu;

        [XmlElement(ElementName = "BlockPropertySettings")]
        public PropBlockConfig propertyBlock;

        [XmlElement(ElementName = "InputSettings")]
        public BindsConfig binds;

        protected override BvConfig GetDefaults()
        {
            return new BvConfig
            {
                VersionID = 6,
                general = GeneralConfig.Defaults,
                menu = PropMenuConfig.Defaults,
                propertyBlock = PropBlockConfig.Defaults,
                binds = BindsConfig.Defaults
            };
        }

        public override void Validate()
        {
            if (VersionID < 6)
            {
                menu.apiHudConfig.resolutionScaling = ApiHudConfig.Defaults.resolutionScaling;
                menu.apiHudConfig.hudScale = ApiHudConfig.Defaults.hudScale;
                menu.apiHudConfig.colors.headerColor = ApiHudConfig.Defaults.colors.headerColor;
                menu.apiHudConfig.colors.listBgColor = ApiHudConfig.Defaults.colors.listBgColor;
                menu.apiHudConfig.colors.selectionBoxColor = ApiHudConfig.Defaults.colors.selectionBoxColor;
            }

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

    public class GeneralConfig : Config<GeneralConfig>
    {
        [XmlElement(ElementName = "CloseIfTargetNotInView")]
        public bool closeIfNotInView;

        [XmlElement(ElementName = "CanOpenIfHandsNotEmpty")]
        public bool canOpenIfHolding;

        protected override GeneralConfig GetDefaults()
        {
            return new GeneralConfig
            {
                closeIfNotInView = true,
                canOpenIfHolding = true
            };
        }

        public override void Validate()
        { }
    }

    public class PropMenuConfig : Config<PropMenuConfig>
    {
        [XmlElement(ElementName = "ForceFallabckHud")]
        public bool forceFallbackHud;

        [XmlElement(ElementName = "ApiHudSettings")]
        public ApiHudConfig apiHudConfig;

        [XmlElement(ElementName = "FallbackHudSettings")]
        public NotifHudConfig fallbackHudConfig;

        protected override PropMenuConfig GetDefaults()
        {
            return new PropMenuConfig
            {
                forceFallbackHud = false,
                apiHudConfig = ApiHudConfig.Defaults,
                fallbackHudConfig = NotifHudConfig.Defaults
            };
        }

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
    /// Stores config information for the Text HUD based menu
    /// </summary>
    public class ApiHudConfig : Config<ApiHudConfig>
    {
        [XmlElement(ElementName = "EnableResolutionScaling")]
        public bool resolutionScaling;

        [XmlElement(ElementName = "HudScale")]
        public float hudScale;

        [XmlElement(ElementName = "MaxVisibleItems")]
        public int maxVisible;

        [XmlElement(ElementName = "ClampHudToScreenEdges")]
        public bool clampHudPos;

        [XmlElement(ElementName = "LockHudToScreenCenter")]
        public bool forceToCenter;

        [XmlElement(ElementName = "ColorsRGB")]
        public Colors colors;

        /// <summary>
        /// Stores configurable text and background colors for Text HUD based menu.
        /// </summary>
        public class Colors : Config<Colors>
        {
            [XmlIgnore]
            public Color listBgColor, selectionBoxColor, headerColor;

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
            public string listBgColorData;

            [XmlElement(ElementName = "SelectionBg")]
            public string selectionBoxColorData;

            [XmlElement(ElementName = "HeaderBg")]
            public string headerColorData;

            protected override Colors GetDefaults()
            {
                return new Colors
                {
                    bodyText = "210,235,245",
                    headerText = "210,235,245",
                    blockIncText = "200,15,15",
                    highlightText = "200,170,0",
                    selectedText = "30,200,30",
                    listBgColor = new Color(60, 65, 70, 190),
                    selectionBoxColor = new Color(41, 54, 62, 255),
                    headerColor = new Color(54, 66, 76, 240)
                };
            }

            /// <summary>
            /// Checks any if fields have invalid values and resets them to the default if necessary.
            /// </summary>
            public override void Validate()
            {
                if (bodyText == null || !Utilities.CanParseColor(bodyText))
                    bodyText = Defaults.bodyText;

                if (blockIncText == null || !Utilities.CanParseColor(blockIncText))
                    blockIncText = Defaults.blockIncText;

                if (highlightText == null || !Utilities.CanParseColor(highlightText))
                    highlightText = Defaults.highlightText;

                if (selectedText == null || !Utilities.CanParseColor(selectedText))
                    selectedText = Defaults.selectedText;

                if (listBgColorData == null || !Utilities.TryParseColor(listBgColorData, out listBgColor))
                {
                    listBgColor = Defaults.listBgColor;
                    listBgColorData = Utilities.GetColorString(listBgColor);
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

        protected override ApiHudConfig GetDefaults()
        {
            return new ApiHudConfig
            {
                resolutionScaling = true,
                hudScale = 0.9f,
                maxVisible = 11,
                clampHudPos = true,
                forceToCenter = false,
                colors = Colors.Defaults
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

            if (colors != null)
                colors.Validate();
            else
                colors = Defaults.colors;
        }
    }

    /// <summary>
    /// Stores config information for the built in Notifications based menu
    /// </summary>
    public class NotifHudConfig : Config<NotifHudConfig>
    {
        [XmlElement(ElementName = "MaxVisibleItems")]
        public int maxVisible;

        protected override NotifHudConfig GetDefaults()
        {
            return new NotifHudConfig
            {
                maxVisible = 8
            };
        }

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
        [XmlIgnore]
        public static KeyBindData[] DefaultBinds
        {
            get
            {
                KeyBindData[] copy = new KeyBindData[defaultBinds.Length];

                for (int n = 0; n < defaultBinds.Length; n++)
                    copy[n] = defaultBinds[n];

                return copy;
            }
        }

        private static readonly KeyBindData[] defaultBinds = new KeyBindData[]
        {
            new KeyBindData("open", new string[] { "control", "middlebutton" }),
            new KeyBindData("close", new string[] { "shift", "middlebutton" }),
            new KeyBindData("select", new string[] { "middlebutton" }),
            new KeyBindData("scrollup", new string[] { "mousewheelup" }),
            new KeyBindData("scrolldown", new string[] { "mousewheeldown" }),
            new KeyBindData("multx", new string[] { "control" }),
            new KeyBindData("multy", new string[] { "shift" }),
            new KeyBindData("multz", new string[] { "control", "shift" })
        };

        [XmlArray("KeyBinds")]
        public KeyBindData[] bindData;

        protected override BindsConfig GetDefaults()
        {
            return new BindsConfig { bindData = DefaultBinds };
        }

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            if (bindData == null || bindData.Length != defaultBinds.Length)
                bindData = DefaultBinds;
        }
    }
}