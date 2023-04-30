using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    using Rendering;
    using Rendering.Client;
    using Rendering.Server;
    using System.Collections;

    /// <summary>
    /// HUD element used to render text.
    /// </summary>
    public class Label : LabelElementBase
    {
        /// <summary>
        /// Text rendered by the label.
        /// </summary>
        public RichText Text { get { return _textBoard.GetText(); } set { _textBoard.SetText(value); } }

        /// <summary>
        /// TextBoard backing the label element.
        /// </summary>
        public override ITextBoard TextBoard { get; }

        /// <summary>
        /// Default formatting used by the label.
        /// </summary>
        public GlyphFormat Format { get { return _textBoard.Format; } set { _textBoard.SetFormatting(value); } }

        /// <summary>
        /// Line formatting mode used by the label.
        /// </summary>
        public TextBuilderModes BuilderMode { get { return _textBoard.BuilderMode; } set { _textBoard.BuilderMode = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text. True by default.
        /// </summary>
        public bool AutoResize { get { return _textBoard.AutoResize; } set { _textBoard.AutoResize = value; } }

        /// <summary>
        /// If true, the text will be vertically centered. True by default.
        /// </summary>
        public bool VertCenterText { get { return _textBoard.VertCenterText; } set { _textBoard.VertCenterText = value; } }

        protected readonly TextBoard _textBoard;

        public Label(HudParentBase parent) : base(parent)
        {
            _textBoard = new TextBoard();
            _textBoard.Format = GlyphFormat.White;
            _textBoard.SetText("NewLabel");
            TextBoard = _textBoard;
        }

        public Label() : this(null)
        { }

        protected override void Draw()
        {
            Vector2 size = (CachedSize - Padding),
                halfSize = .5f * size;
            BoundingBox2 box = new BoundingBox2(Position - halfSize, Position + halfSize);
            bool autoResize = AutoResize;

            if (!autoResize)
                _textBoard.FixedSize = size;

            if (maskingBox != null)
                _textBoard.Draw(box, maskingBox.Value, HudSpace.PlaneToWorldRef);
            else
                _textBoard.Draw(box, CroppedBox.defaultMask, HudSpace.PlaneToWorldRef);

            if (autoResize)
                UnpaddedSize = _textBoard.TextSize;
        }
    }
}
