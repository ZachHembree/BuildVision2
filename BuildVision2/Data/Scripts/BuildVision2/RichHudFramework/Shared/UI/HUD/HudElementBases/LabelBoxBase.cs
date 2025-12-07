using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Abstract base class for UI elements that combine a text label with a textured background.
		/// </summary>
		public abstract class LabelBoxBase : HudElementBase
		{
			/// <summary>
			/// Gets or sets the total dimensions of the text element. 
			/// </summary>
			public abstract Vector2 TextSize { get; set; }

			/// <summary>
			/// Gets or sets the padding applied to the text element, offsetting it from the edges of the background.
			/// </summary>
			public abstract Vector2 TextPadding { get; set; }

			/// <summary>
			/// If true, the element's size is driven by the size of the text content. 
			/// If false, the element's size is set manually, and text may be clipped if the bounds are too small.
			/// </summary>
			public abstract bool AutoResize { get; set; }

			/// <summary>
			/// If true, the background size will match the size of the text element exactly. 
			/// <para>If false, the background size is clamped so it cannot be smaller than the text element, but can be larger.</para>
			/// <para>Note: This property is ignored if <see cref="AutoResize"/> is disabled.</para>
			/// </summary>
			public bool FitToTextElement { get; set; }

			/// <summary>
			/// Gets or sets the color of the background texture.
			/// </summary>
			public virtual Color Color { get { return Background.Color; } set { Background.Color = value; } }

			/// <summary>
			/// The textured, tintable background element drawn behind the text.
			/// </summary>
			public readonly TexturedBox Background;

			public LabelBoxBase(HudParentBase parent) : base(parent)
			{
				Background = new TexturedBox(this)
				{
					DimAlignment = DimAlignments.UnpaddedSize,
				};

				FitToTextElement = true;
				Color = Color.Gray;
				UnpaddedSize = new Vector2(50f);
			}

			/// <summary>
			/// Recalculates the size of the element based on the text dimensions if <see cref="AutoResize"/> is active.
			/// </summary>
			/// <exclude/>
			protected override void Measure()
			{
				if (AutoResize)
				{
					if (FitToTextElement)
						UnpaddedSize = TextSize;
					else
						UnpaddedSize = Vector2.Max(UnpaddedSize, TextSize);
				}
			}

			/// <summary>
			/// Enforces the element's size onto the text element if <see cref="AutoResize"/> is disabled.
			/// </summary>
			/// <exclude/>
			protected override void Layout()
			{
				if (!AutoResize)
				{
					TextSize = UnpaddedSize;
				}
			}
		}
	}
}