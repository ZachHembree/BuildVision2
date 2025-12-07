using System;
using VRageMath;

namespace RichHudFramework.UI
{
	using Client;
	using Server;
	using static NodeConfigIndices;

	/// <summary>
	/// Core mouse interaction component for clickable UI elements.
	/// Handles cursor enter/exit, left/right clicks, tooltip registration, and automatic focus acquisition on click.
	/// </summary>
	public class MouseInputElement : HudElementBase, IMouseInput
	{
		/// <summary>
		/// UI element that owns the input, used for event callbacks.
		/// </summary>
		public IFocusHandler FocusHandler { get; protected set; }

		/// <summary>
		/// Invoked when the mouse cursor enters the element's interactive area.
		/// </summary>
		public event EventHandler CursorEntered;

		/// <summary>
		/// Invoked when the mouse cursor leaves the element's interactive area.
		/// </summary>
		public event EventHandler CursorExited;

		/// <summary>
		/// Invoked when the element is clicked with the left mouse button.
		/// </summary>
		public event EventHandler LeftClicked;

		/// <summary>
		/// Invoked when the left mouse button is released over the element.
		/// </summary>
		public event EventHandler LeftReleased;

		/// <summary>
		/// Invoked when the element is clicked with the right mouse button.
		/// </summary>
		public event EventHandler RightClicked;

		/// <summary>
		/// Invoked when the right mouse button is released over the element.
		/// </summary>
		public event EventHandler RightReleased;

		/// <summary>
		/// Invoked when the mouse cursor enters the element's interactive area. Event initializer.
		/// </summary>
		public EventHandler CursorEnteredCallback { set { CursorEntered += value; } }

		/// <summary>
		/// Invoked when the mouse cursor leaves the element's interactive area. Event initializer.
		/// </summary>
		public EventHandler CursorExitedCallback { set { CursorExited += value; } }
		/// <summary>
		/// Invoked when the element is clicked with the left mouse button. Event initializer.
		/// </summary>
		public EventHandler LeftClickedCallback { set { LeftClicked += value; } }

		/// <summary>
		/// Invoked when the left mouse button is released over the element. Event initializer.
		/// </summary>
		public EventHandler LeftReleasedCallback { set { LeftReleased += value; } }

		/// <summary>
		/// Invoked when the element is clicked with the right mouse button. Event initializer.
		/// </summary>
		public EventHandler RightClickedCallback { set { RightClicked += value; } }

		/// <summary>
		/// Invoked when the right mouse button is released over the element. Event initializer.
		/// </summary>
		public EventHandler RightReleasedCallback { set { RightReleased += value; } }

        /// <summary>
        /// If true, the input element will temporarily show the cursor while it's enabled.
		/// <para>Uses <see cref="HudMain.EnableCursorTemp"></see>.</para>
        /// </summary>
        public bool RequestCursor { get; set; }

        /// <summary>
        /// Optional tooltip text shown when the element is moused over.
        /// </summary>
        public ToolTip ToolTip { get; set; }

		/// <summary>
		/// Returns true if the element is currently being held down with the left mouse button.
		/// </summary>
		public bool IsLeftClicked { get; private set; }

		/// <summary>
		/// Returns true if the element is currently being held down with the right mouse button.
		/// </summary>
		public bool IsRightClicked { get; private set; }

		/// <summary>
		/// Returns true if the element was just clicked with the left mouse button this frame.
		/// </summary>
		public bool IsNewLeftClicked { get; private set; }

		/// <summary>
		/// Returns true if the element was just clicked with the right mouse button this frame.
		/// </summary>
		public bool IsNewRightClicked { get; private set; }

		/// <summary>
		/// Returns true if the element was just released after being left-clicked this frame.
		/// </summary>
		public bool IsLeftReleased { get; private set; }

		/// <summary>
		/// Returns true if the element was just released after being right-clicked this frame.
		/// </summary>
		public bool IsRightReleased { get; private set; }

		private bool mouseCursorEntered;

		public MouseInputElement(HudParentBase parent) : base(parent)
		{
			FocusHandler = (parent as IFocusableElement)?.FocusHandler;
			UseCursor = true;
			ShareCursor = true;
			DimAlignment = DimAlignments.UnpaddedSize;
		}

		public MouseInputElement() : this(null)
		{ }

		/// <summary>
		/// Clears all subscribers to mouse input events.
		/// </summary>
		public void ClearSubscribers()
		{
			CursorEntered = null;
			CursorExited = null;
			LeftClicked = null;
			LeftReleased = null;
			RightClicked = null;
			RightReleased = null;
		}

		/// <summary>
		/// Updates cursor hit testing for the element
		/// </summary>
		/// <exclude/>
		protected override void InputDepth()
		{
			if (HudSpace.IsFacingCamera)
			{
				Vector3 cursorPos = HudSpace.CursorPos;
				Vector2 halfSize = Vector2.Max(CachedSize, new Vector2(MinMouseBounds)) * .5f;
				BoundingBox2 box = new BoundingBox2(Position - halfSize, Position + halfSize);
				bool mouseInBounds;

				if (MaskingBox == null)
				{
					mouseInBounds = box.Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains
						|| (IsLeftClicked || IsRightClicked);
				}
				else
				{
					mouseInBounds = box.Intersect(MaskingBox.Value).Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains
						|| (IsLeftClicked || IsRightClicked);
				}

				if (mouseInBounds)
				{
					_config[StateID] |= (uint)HudElementStates.IsMouseInBounds;
					HudMain.Cursor.TryCaptureHudSpace(cursorPos.Z, HudSpace.GetHudSpaceFunc);
				}
			}
		}

		/// <summary>
		/// Updates click input state and fires input events
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			FocusHandler = (Parent as IFocusableElement)?.FocusHandler;
            var owner = (object)(FocusHandler?.InputOwner) ?? Parent;

			if (RequestCursor)
				HudMain.EnableCursorTemp();

            if (IsMousedOver)
			{
				if (!mouseCursorEntered)
				{
					mouseCursorEntered = true;
					CursorEntered?.Invoke(owner, EventArgs.Empty);
				}

				if (SharedBinds.LeftButton.IsNewPressed)
				{
					FocusHandler?.GetInputFocus();
					LeftClick();
				}
				else
					IsNewLeftClicked = false;

				if (SharedBinds.RightButton.IsNewPressed)
				{
					FocusHandler?.GetInputFocus();
					RightClick();
				}
				else
					IsNewRightClicked = false;

				if (ToolTip != null)
					HudMain.Cursor.RegisterToolTip(ToolTip);
			}
			else
			{
				if (mouseCursorEntered)
				{
					mouseCursorEntered = false;
					CursorExited?.Invoke(owner, EventArgs.Empty);
				}

				bool hasFocus = FocusHandler?.HasFocus ?? false;

				if (hasFocus && (SharedBinds.LeftButton.IsNewPressed || SharedBinds.RightButton.IsNewPressed))
					FocusHandler.ReleaseFocus();

				IsNewLeftClicked = false;
				IsNewRightClicked = false;
			}

			if (!SharedBinds.LeftButton.IsPressed && IsLeftClicked)
			{
				LeftReleased?.Invoke(owner, EventArgs.Empty);
				IsLeftReleased = true;
				IsLeftClicked = false;
			}
			else
				IsLeftReleased = false;

			if (!SharedBinds.RightButton.IsPressed && IsRightClicked)
			{
				RightReleased?.Invoke(owner, EventArgs.Empty);
				IsRightReleased = true;
				IsRightClicked = false;
			}
			else
				IsRightReleased = false;
		}

		/// <summary>
		/// Invokes left click event
		/// </summary>
		public virtual void LeftClick()
		{
            var owner = (object)(FocusHandler?.InputOwner) ?? Parent;
            LeftClicked?.Invoke(owner, EventArgs.Empty);
			IsLeftClicked = true;
			IsNewLeftClicked = true;
			IsLeftReleased = false;
		}

		/// <summary>
		/// Invokes right click event
		/// </summary>
		public virtual void RightClick()
		{
            var owner = (object)(FocusHandler?.InputOwner) ?? Parent;
            RightClicked?.Invoke(owner, EventArgs.Empty);
			IsRightClicked = true;
			IsNewRightClicked = true;
			IsRightReleased = false;
		}
	}
}