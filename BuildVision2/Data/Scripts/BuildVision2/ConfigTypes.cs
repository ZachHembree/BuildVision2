using DarkHelmet.IO;
using DarkHelmet.UI;
using System.Xml.Serialization;
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
        public PropBlockConfig block;

        [XmlElement(ElementName = "InputSettings")]
        public BindsConfig binds;

        protected override BvConfig GetDefaults()
        {
            return new BvConfig
            {
                VersionID = 6,
                general = GeneralConfig.Defaults,
                menu = PropMenuConfig.Defaults,
                block = PropBlockConfig.Defaults,
                binds = BindsConfig.Defaults
            };
        }

        public override void Validate()
        {
            if (VersionID < 6)
                menu = PropMenuConfig.Defaults;

            if (VersionID < 5)
                block = PropBlockConfig.Defaults;

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

            if (block != null)
                block.Validate();
            else
                block = PropBlockConfig.Defaults;
        }
    }

    public class GeneralConfig : Config<GeneralConfig>
    {
        [XmlElement(ElementName = "CloseIfTargetNotInView")]
        public bool closeIfNotInView;

        [XmlElement(ElementName = "CanOpenIfHandsNotEmpty")]
        public bool canOpenIfHolding;

        [XmlElement(ElementName = "maxOpenRange")]
        public double maxOpenRange;

        [XmlElement(ElementName = "maxControlRange")]
        public double maxControlRange;

        protected override GeneralConfig GetDefaults()
        {
            return new GeneralConfig
            {
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
                maxOpenRange = Utils.Math.Clamp(maxOpenRange, 2.5d, 20d);

            if (maxControlRange == 0d)
                maxControlRange = Defaults.maxControlRange;
            else
                maxControlRange = Utils.Math.Clamp(maxControlRange, maxOpenRange, 60d);
        }
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

        [XmlElement(ElementName = "UseCustomHudPosition")]
        public bool useCustomPos;

        [XmlElement(ElementName = "HudPosition")]
        public Vector2 hudPos;

        [XmlElement(ElementName = "ColorsRGB")]
        public Colors colors;

        /// <summary>
        /// Stores configurable text and background colors for Text HUD based menu.
        /// </summary>
        public class Colors : Config<Colors>
        {
            [XmlElement(ElementName = "BodyText")]
            public ColorData bodyText;

            [XmlElement(ElementName = "HeaderText")]
            public ColorData headerText;

            [XmlElement(ElementName = "BlockIncompleteText")]
            public ColorData blockIncText;

            [XmlElement(ElementName = "HighlightText")]
            public ColorData highlightText;

            [XmlElement(ElementName = "SelectedText")]
            public ColorData selectedText;

            [XmlElement(ElementName = "ListBg")]
            public ColorData listBg;

            [XmlElement(ElementName = "SelectionBg")]
            public ColorData selectionBox;

            [XmlElement(ElementName = "HeaderBg")]
            public ColorData header;

            protected override Colors GetDefaults()
            {
                return new Colors
                {
                    bodyText = new ColorData(210, 235, 245),
                    headerText = new ColorData(210, 235, 245),
                    blockIncText = new ColorData(200, 35, 35),
                    highlightText = new ColorData(220, 190, 20),
                    selectedText = new ColorData(50, 200, 50),
                    listBg = new ColorData(70, 78, 86, 204),
                    selectionBox = new ColorData(41, 54, 62, 255),
                    header = new ColorData(41, 54, 62, 230)
                };
            }

            /// <summary>
            /// Checks any if fields have invalid values and resets them to the default if necessary.
            /// </summary>
            public override void Validate()
            {
                bodyText.Validate(Defaults.bodyText);
                headerText.Validate(Defaults.headerText);
                blockIncText.Validate(Defaults.blockIncText);
                highlightText.Validate(Defaults.highlightText);
                selectedText.Validate(Defaults.selectedText);
                listBg.Validate(Defaults.listBg);
                selectionBox.Validate(Defaults.selectionBox);
                header.Validate(Defaults.header);
            }

            public class ColorData
            {
                [XmlIgnore]
                public string data;
                [XmlIgnore]
                public Color color;

                public ColorData()
                { }

                public ColorData(int r, int g, int b)
                {
                    color = new Color(r, g, b);
                    data = Utils.Color.GetColorString(color, false);
                }

                public ColorData(int r, int g, int b, int a)
                {
                    color = new Color(r, g, b, a);
                    data = Utils.Color.GetColorString(color);
                }

                public void Validate(ColorData def)
                {
                    if (data == null || !Utils.Color.TryParseColor(data, out color))
                    {
                        color = def.color;
                        data = def.data;
                    }
                }

                public override string ToString() =>
                    Utils.Color.GetColorString(color);
            }
        }

        protected override ApiHudConfig GetDefaults()
        {
            return new ApiHudConfig
            {
                resolutionScaling = true,
                hudScale = 1f,
                maxVisible = 14,
                clampHudPos = true,
                useCustomPos = false,
                hudPos = new Vector2(-0.97083337604999542f, 0.95370364189147949f),
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
                maxVisible = 7
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
        public static BindData[] DefaultBinds
        {
            get
            {
                BindData[] copy = new BindData[defaultBinds.Length];

                for (int n = 0; n < defaultBinds.Length; n++)
                    copy[n] = defaultBinds[n];

                return copy;
            }
        }

        private static readonly BindData[] defaultBinds = new BindData[]
        {
            new BindData("open", new string[] { "control", "middlebutton" }),
            new BindData("close", new string[] { "shift", "middlebutton" }),
            new BindData("select", new string[] { "middlebutton" }),
            new BindData("scrollup", new string[] { "mousewheelup" }),
            new BindData("scrolldown", new string[] { "mousewheeldown" }),
            new BindData("multx", new string[] { "control" }),
            new BindData("multy", new string[] { "shift" }),
            new BindData("multz", new string[] { "control", "shift" })
        };

        [XmlArray("KeyBinds")]
        public BindData[] bindData;

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