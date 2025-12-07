using System;
using VRageMath;
using RichHudFramework.UI.Rendering;

namespace RichHudFramework.UI
{
	using Client;
	using Server;

	/// <summary>
	/// Base class for a standard window element featuring a header, body, and border. 
	/// Includes built-in support for mouse dragging, edge resizing, and focus management.
	/// </summary>
	public abstract class WindowBase : HudElementBase, IClickableElement
	{
		/// <summary>
		/// Gets or sets the text displayed in the window's header.
		/// </summary>
		public RichText HeaderText { get { return HeaderBuilder.GetText(); } set { HeaderBuilder.SetText(value); } }

		/// <summary>
		/// Exposes the <see cref="ITextBuilder"/> used to format and manipulate the header text.
		/// </summary>
		public ITextBuilder HeaderBuilder => header.TextBoard;

		/// <summary>
		/// Gets or sets the color of both the window border and the header background.
		/// </summary>
		public virtual Color BorderColor
		{
			get { return header.Color; }
			set
			{
				header.Color = value;
				border.Color = value;
			}
		}

		/// <summary>
		/// Gets or sets the background color of the window's body area.
		/// </summary>
		public virtual Color BodyColor { get { return windowBg.Color; } set { windowBg.Color = value; } }

		/// <summary>
		/// Gets or sets the minimum allowable dimensions for the window during resizing.
		/// </summary>
		public Vector2 MinimumSize { get; set; }

		/// <summary>
		/// Determines if the user can resize the window by dragging its edges.
		/// </summary>
		public bool AllowResizing { get; set; }

		/// <summary>
		/// Determines if the user can reposition the window by clicking and dragging the header.
		/// </summary>
		public bool CanDrag { get; set; }

		/// <summary>
		/// Indicates whether the window is currently active.
		/// </summary>
		public bool WindowActive { get; protected set; }

		/// <summary>
		/// Indicates whether the mouse cursor is currently hovering over the window or its resize padding.
		/// </summary>
		public override bool IsMousedOver => resizeInput.IsMousedOver;

		/// <summary>
		/// The generic mouse input handler for the window
		/// </summary>
		public IMouseInput MouseInput { get; }

		/// <summary>
		/// Handles the element's input focus state and registration.
		/// </summary>
		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// The UI element representing the window's header bar.
		/// </summary>
		public readonly LabelBoxButton header;

		/// <summary>
		/// The container element for the window's main content area.
		/// </summary>
		public readonly HudElementBase body;

		/// <summary>
		/// The element responsible for rendering the window's border outline.
		/// </summary>
		public readonly BorderBox border;

		protected readonly MouseInputElement inputInner, resizeInput;
		protected readonly TexturedBox windowBg;

		/// <summary>
		/// The distance from the corner within which a mouse drag triggers diagonal resizing.
		/// </summary>
		protected float cornerSize = 16f;
		protected bool canMoveWindow;
		protected Vector2 resizeDir, cursorOffset;

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowBase"/> class.
		/// </summary>
		/// <param name="parent">The parent HUD element.</param>
		public WindowBase(HudParentBase parent) : base(parent)
		{
			header = new LabelBoxButton(this)
			{
				DimAlignment = DimAlignments.Width,
				Height = 32f,
				ParentAlignment = ParentAlignments.InnerTop,
				ZOffset = 1,
				Format = GlyphFormat.White.WithAlignment(TextAlignment.Center),
				HighlightEnabled = false,
				AutoResize = false,
			};

			body = new EmptyHudElement(this)
			{
				ParentAlignment = ParentAlignments.InnerBottom,
			};

			windowBg = new TexturedBox(this)
			{
				DimAlignment = DimAlignments.Size,
				ZOffset = -2,
			};

			border = new BorderBox(this)
			{
				ZOffset = 1,
				Thickness = 1f,
				DimAlignment = DimAlignments.Size,
			};

			FocusHandler = new InputFocusHandler(this);
			resizeInput = new MouseInputElement(this)
			{
				ZOffset = sbyte.MaxValue,
				Padding = new Vector2(16f),
				DimAlignment = DimAlignments.Size,
				CanIgnoreMasking = true
			};
			inputInner = new MouseInputElement(resizeInput)
			{
				DimAlignment = DimAlignments.UnpaddedSize,
			};

			resizeDir = Vector2.Zero;
			AllowResizing = true;
			CanDrag = true;
			UseCursor = true;
			ShareCursor = false;
			IsMasking = true;
			MinimumSize = new Vector2(200f, 200f);
			MouseInput = resizeInput;

			GetWindowFocus();
		}

		/// <summary>
		/// Updates the layout of the window's body relative to the header size.
		/// </summary>
		protected override void Layout()
		{
			body.Height = UnpaddedSize.Y - header.Height;
			body.Width = UnpaddedSize.X;
		}

		/// <summary>
		/// Calculates and applies the new window size and position based on the drag delta and resize direction.
		/// </summary>
		/// <param name="cursorPos">The current position of the cursor.</param>
		protected void Resize(Vector2 cursorPos)
		{
			Vector2 pos = Origin + Offset,
				delta = resizeDir * (cursorPos - pos),
				size = CachedSize;

			if (delta.X > 0f)
			{
				delta.X = Math.Max(delta.X, .5f * MinimumSize.X);
				size.X = .5f * size.X + delta.X;
				pos.X = ((resizeDir.X * delta.X) + pos.X) + (-resizeDir.X * .5f * size.X);
			}

			if (delta.Y > 0f)
			{
				delta.Y = Math.Max(delta.Y, .5f * MinimumSize.Y);
				size.Y = .5f * size.Y + delta.Y;
				pos.Y = ((resizeDir.Y * delta.Y) + pos.Y) + (-resizeDir.Y * .5f * size.Y);
			}

			Size = size;
			Offset = pos - Origin;
		}

		/// <summary>
		/// Handles mouse input for resizing, dragging, and focus acquisition.
		/// </summary>
		/// <param name="cursorPos">The current cursor position.</param>
		protected override void HandleInput(Vector2 cursorPos)
		{
			if (IsMousedOver)
			{
				if (SharedBinds.LeftButton.IsNewPressed && !WindowActive)
					GetWindowFocus();
			}

			if (AllowResizing && resizeInput.IsNewLeftClicked && !inputInner.IsMousedOver)
			{
				Vector2 pos = Origin + Offset,
						delta = cursorPos - pos;

				resizeDir = Vector2.Zero;

				if (Width - (2f * Math.Abs(delta.X)) <= cornerSize)
					resizeDir.X = (delta.X >= 0f) ? 1f : -1f;

				if (Height - (2f * Math.Abs(delta.Y)) <= cornerSize)
					resizeDir.Y = (delta.Y >= 0f) ? 1f : -1f;
			}
			else if (CanDrag && header.MouseInput.IsNewLeftClicked)
			{
				canMoveWindow = true;
				cursorOffset = (Origin + Offset) - cursorPos;
			}

			if ((resizeDir != Vector2.Zero) || canMoveWindow)
			{
				if (!SharedBinds.LeftButton.IsPressed)
				{
					canMoveWindow = false;
					resizeDir = Vector2.Zero;
				}
			}

			if (!WindowActive)
			{
				canMoveWindow = false;
				resizeDir = Vector2.Zero;
			}

			if (canMoveWindow)
				Offset = cursorPos + cursorOffset - Origin;

			if (resizeDir != Vector2.Zero)
				Resize(cursorPos);
		}

		/// <summary>
		/// Brings the window to the foreground (top Z-layer) and captures input focus. 
		/// Overriding methods must call the base implementation.
		/// </summary>
		public virtual void GetWindowFocus()
		{
			OverlayOffset = HudMain.GetFocusOffset(LoseWindowFocus);
			WindowActive = true;
		}

		/// <summary>
		/// Callback triggered when the window loses focus to another element. 
		/// Overriding methods must call the base implementation.
		/// </summary>
		/// <param name="newLayer">The new Z-offset layer assigned to this window.</param>
		protected virtual void LoseWindowFocus(byte newLayer)
		{
			OverlayOffset = newLayer;
			WindowActive = false;
		}
	}
}