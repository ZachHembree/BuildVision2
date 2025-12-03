using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Named checkbox designed to mimic the appearance of checkboxes used in the SE terminal.
	/// <para>Adds a label to <see cref="BorderedCheckBox"/>.</para>
	/// <para>Formatting temporarily changes when it gains input focus.</para>
	/// </summary>
	public class NamedCheckBox : HudElementBase, IClickableElement
    {
		/// <summary>
		/// Invoked when the current value (<see cref="IsBoxChecked"/>) changes
		/// </summary>
		public event EventHandler ValueChanged
		{
			add { checkbox.ValueChanged += value; }
			remove { checkbox.ValueChanged -= value; }
		}

		/// <summary>
		/// Registers a value (<see cref="IsBoxChecked"/>) update callback. Useful in initializers.
		/// </summary>
		public EventHandler UpdateValueCallback
		{
			set { checkbox.ValueChanged += value; }
		}

		/// <summary>
		/// Text rendered by the label.
		/// </summary>
		public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

        /// <summary>
        /// Default formatting used by the label.
        /// </summary>
        public GlyphFormat Format { get { return name.TextBoard.Format; } set { name.TextBoard.Format = value; } }

        /// <summary>
        /// Size of the text element sans padding.
        /// </summary>
        public Vector2 TextSize { get { return name.Size; } set { name.Size = value; } }

        /// <summary>
        /// Padding applied to the text element.
        /// </summary>
        public Vector2 TextPadding { get { return name.Padding; } set { name.Padding = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public bool AutoResize 
        { 
            get { return name.AutoResize; } 
            set 
            { 
                name.AutoResize = value;
                layout[0].AlignAxisScale = value ? 0f : 1f;
            } 
        }

        /// <summary>
        /// Line formatting mode used by the label.
        /// </summary>
        public TextBuilderModes BuilderMode { get { return name.BuilderMode; } set { name.BuilderMode = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return name.VertCenterText; } set { name.VertCenterText = value; } }

        /// <summary>
        /// TextBoard backing the label element.
        /// </summary>
        public ITextBuilder NameBuilder => name.TextBoard;

		/// <summary>
		/// Interface for managing gaining/losing input focus
		/// </summary>
		public IFocusHandler FocusHandler => checkbox.FocusHandler;

		/// <summary>
		/// Checkbox mouse input
		/// </summary>
		public IMouseInput MouseInput => checkbox.MouseInput;

        /// <summary>
        /// Indicates whether or not the box is checked.
        /// </summary>
        public bool IsBoxChecked { get { return checkbox.IsBoxChecked; } set { checkbox.IsBoxChecked = value; } }

        /// <summary>
        /// Label to the left of the checkbox
        /// </summary>
        /// <exclude/>
        protected readonly Label name;

        /// <summary>
        /// Checkbox button
        /// </summary>
        /// <exclude/>
		protected readonly BorderedCheckBox checkbox;

        /// <summary>
        /// Stacking container for name and checkbox layout
        /// </summary>
        /// <exclude/>
		protected readonly HudChain layout;

        public NamedCheckBox(HudParentBase parent) : base(parent)
        {
            name = new Label()
            {
                Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Right),
                Text = "NewCheckbox"
            };

            checkbox = new BorderedCheckBox();

            layout = new HudChain(false, this)
            {
                DimAlignment = DimAlignments.UnpaddedSize,
                Spacing = 17f,
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.AlignMembersCenter,
                CollectionContainer = { { name, 0f }, { checkbox, 0f } }
            };

            FocusHandler.InputOwner = this;
            AutoResize = true;
            Size = new Vector2(250f, 37f);
        }

		public NamedCheckBox() : this(null)
		{ }

		/// <summary>
		/// Updates the size of the element to fit the checkbox and label if autoresize is enabled
		/// </summary>
		/// <exclude/>
		protected override void Measure()
        {
            if (AutoResize)
                UnpaddedSize = layout.UnpaddedSize + layout.Padding;
        }
    }
}