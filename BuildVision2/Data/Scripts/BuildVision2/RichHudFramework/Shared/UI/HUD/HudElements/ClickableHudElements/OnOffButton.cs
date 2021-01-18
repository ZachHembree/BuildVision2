using System;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI.Server
{
    /// <summary>
    /// A pair of horizontally aligned on and off bordered buttons used to indicate a boolean value. Made to
    /// resemble on/off button used in the SE terminal, sans name tag.
    /// </summary>
    public class OnOffButton : HudElementBase
    {   
        /// <summary>
        /// Distance between the on and off buttons
        /// </summary>
        public float ButtonSpacing { get { return buttonChain.Spacing; } set { buttonChain.Spacing = value; } }

        /// <summary>
        /// Color of the border surrounding the on and off buttons
        /// </summary>
        public Color BorderColor { get { return on.BorderColor; } set { on.BorderColor = value; off.BorderColor = value; } }

        /// <summary>
        /// Color of the highlight border used to indicate the current selection
        /// </summary>
        public Color HighlightBorderColor { get { return selectionHighlight.Color; } set { selectionHighlight.Color = value; } }

        /// <summary>
        /// On button text
        /// </summary>
        public RichText OnText { get { return on.Text; } set { on.Text = value; } }

        /// <summary>
        /// Off button text
        /// </summary>
        public RichText OffText { get { return off.Text; } set { off.Text = value; } }

        /// <summary>
        /// Default glyph format used by the on and off buttons
        /// </summary>
        public GlyphFormat Format { get { return on.Format; } set { on.Format = value; off.Format = value; } }

        /// <summary>
        /// Current value of the on/off button
        /// </summary>
        public bool Value { get; set; }

        protected readonly BorderedButton on, off;
        protected readonly HudChain buttonChain;
        protected readonly BorderBox selectionHighlight;

        public OnOffButton(HudParentBase parent) : base(parent)
        {
            on = new BorderedButton()
            {
                Text = "On",
                Padding = Vector2.Zero,
                Size = new Vector2(71f, 49f),
                HighlightEnabled = true,
            };

            on.BorderThickness = 2f;

            off = new BorderedButton()
            {
                Text = "Off",
                Padding = Vector2.Zero,
                Size = new Vector2(71f, 49f),
                HighlightEnabled = true,
            };

            off.BorderThickness = 2f;

            buttonChain = new HudChain(false, this)
            {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Spacing = 9f,
                CollectionContainer = { on, off }
            };

            on.MouseInput.OnLeftClick += ToggleValue;
            off.MouseInput.OnLeftClick += ToggleValue;

            selectionHighlight = new BorderBox(buttonChain)
            { 
                Color = Color.White 
            };

            Size = new Vector2(250f, 50f);
        }

        public OnOffButton() : this(null)
        { }

        private void ToggleValue(object sender, EventArgs args)
        {
            Value = !Value;
        }

        protected override void Layout()
        {
            Vector2 buttonSize = cachedSize - cachedPadding;
            buttonSize.X = buttonSize.X / 2f - buttonChain.Spacing;
            buttonChain.MemberMaxSize = buttonSize;

            if (Value)
            {
                selectionHighlight.Size = buttonSize;
                selectionHighlight.Offset = on.Offset;
            }
            else
            {
                selectionHighlight.Size = buttonSize;
                selectionHighlight.Offset = off.Offset;
            }
        }
    }
}