using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		using static RichHudFramework.UI.NodeConfigIndices;
		using Server;
		using Client;
		using Internal;

		/// <summary>
		/// Base type for all UI elements with definite size and position. Extends HudParentBase and HudNodeBase.
		/// </summary>
		public abstract class HudElementBase : HudNodeBase, IReadOnlyHudElement
		{
			protected const float minMouseBounds = 8f;

			/// <summary>
			/// Size of the element. Units in pixels with HudMain.Root.
			/// </summary>
			public Vector2 Size
			{
				get { return UnpaddedSize + Padding; }
				set
				{
					if (value.X > Padding.X)
						value.X -= Padding.X;

					if (value.Y > Padding.Y)
						value.Y -= Padding.Y;

					UnpaddedSize = value;
				}
			}

			/// <summary>
			/// Width of the element. Units in pixels by HudMain.Root.
			/// </summary>
			public float Width
			{
				get { return UnpaddedSize.X + Padding.X; }
				set
				{
					if (value > Padding.X)
						value -= Padding.X;

					UnpaddedSize = new Vector2(value, UnpaddedSize.Y);
				}
			}

			/// <summary>
			/// Height of the element. Units in pixels by HudMain.Root.
			/// </summary>
			public float Height
			{
				get { return UnpaddedSize.Y + Padding.Y; }
				set
				{
					if (value > Padding.Y)
						value -= Padding.Y;

					UnpaddedSize = new Vector2(UnpaddedSize.X, value);
				}
			}

			/// <summary>
			/// Border size. Included in total element size.
			/// </summary>
			public Vector2 Padding { get; set; }

			/// <summary>
			/// Element size without padding
			/// </summary>
			public Vector2 UnpaddedSize { get; set; }

			/// <summary>
			/// Starting position of the hud element. Starts in the center of the parent node 
			/// by default. This behavior can be modified with ParentAlignment flags.
			/// </summary>
			public Vector2 Origin { get; private set; }

			/// <summary>
			/// Position of the center of the UI element relative to its origin.
			/// </summary>
			public Vector2 Offset { get; set; }

			/// <summary>
			/// Current position of the center of the UI element. Origin + Offset.
			/// </summary>
			public Vector2 Position { get; private set; }

			/// <summary>
			/// Determines the starting position of the hud element relative to its parent.
			/// </summary>
			public ParentAlignments ParentAlignment { get; set; }

			/// <summary>
			/// Determines how/if an element will copy its parent's dimensions. 
			/// </summary>
			public DimAlignments DimAlignment { get; set; }

			/// <summary>
			/// Enables or disables cursor input and capture
			/// </summary>
			public bool UseCursor
			{
				get { return (Config[StateID] & (uint)HudElementStates.CanUseCursor) > 0; }
				set
				{
					if (value)
						Config[StateID] |= (uint)HudElementStates.CanUseCursor;
					else
						Config[StateID] &= ~(uint)HudElementStates.CanUseCursor;

					if (value && _dataHandle[0].Item3.Item3 == null)
						_dataHandle[0].Item3.Item3 = BeginInput;
				}
			}

			/// <summary>
			/// If set to true the hud element will share the cursor with other elements.
			/// </summary>
			public bool ShareCursor
			{
				get { return (Config[StateID] & (uint)HudElementStates.CanShareCursor) > 0; }
				set
				{
					if (value)
						Config[StateID] |= (uint)HudElementStates.CanShareCursor;
					else
						Config[StateID] &= ~(uint)HudElementStates.CanShareCursor;
				}
			}

			/// <summary>
			/// If set to true, the hud element will act as a clipping mask for child elements.
			/// False by default. Masking parent elements can still affect non-masking children.
			/// </summary>
			public bool IsMasking
			{
				get { return (Config[StateID] & (uint)HudElementStates.IsMasking) > 0; }
				set
				{
					if (value)
						Config[StateID] |= (uint)HudElementStates.IsMasking;
					else
						Config[StateID] &= ~(uint)HudElementStates.IsMasking;
				}
			}

			/// <summary>
			/// If set to true, the hud element will treat its parent as a clipping mask, whether
			/// it's configured as a mask or not.
			/// </summary>
			public bool IsSelectivelyMasked
			{
				get { return (Config[StateID] & (uint)HudElementStates.IsSelectivelyMasked) > 0; }
				set
				{
					if (value)
						Config[StateID] |= (uint)HudElementStates.IsSelectivelyMasked;
					else
						Config[StateID] &= ~(uint)HudElementStates.IsSelectivelyMasked;
				}
			}

			/// <summary>
			/// If set to true, then the element can ignore any bounding masks imposed by its parents.
			/// Superceeds selective masking flag.
			/// </summary>
			public bool CanIgnoreMasking
			{
				get { return (Config[StateID] & (uint)HudElementStates.CanIgnoreMasking) > 0; }
				set
				{
					if (value)
						Config[StateID] |= (uint)HudElementStates.CanIgnoreMasking;
					else
						Config[StateID] &= ~(uint)HudElementStates.CanIgnoreMasking;
				}
			}

			/// <summary>
			/// Indicates whether or not the element is capturing the cursor.
			/// </summary>
			public virtual bool IsMousedOver => (Config[StateID] & (uint)HudElementStates.IsMousedOver) > 0;

			/// <summary>
			/// Last known final size, and the next size that will be used on Draw.
			/// </summary>
			protected Vector2 CachedSize { get; private set; }

			/// <summary>
			/// Origin offset used internally for parent alignment
			/// </summary>
			protected Vector2 OriginAlignment { get; private set; }

			protected BoundingBox2? maskingBox;

			/// <summary>
			/// Initializes a new UI element attached to the given parent.
			/// </summary>
			public HudElementBase(HudParentBase parent) : base(parent)
			{
				DimAlignment = DimAlignments.None;
				ParentAlignment = ParentAlignments.Center;

				Origin = Vector2.Zero;
				Position = Vector2.Zero;
				OriginAlignment = Vector2.Zero;
			}

			/// <summary>
			/// Update hook for testing cursor bounding and depth tests. 
			/// 
			/// Updates in back-to-front order after Draw(). Elements on the bottom update first, and elements 
			/// on top update last.
			/// </summary>
			protected override void InputDepth()
			{
				if (HudSpace.IsFacingCamera)
				{
					Vector3 cursorPos = HudSpace.CursorPos;
					Vector2 halfSize = Vector2.Max(CachedSize, new Vector2(minMouseBounds)) * .5f;
					BoundingBox2 box = new BoundingBox2(Position - halfSize, Position + halfSize);
					bool mouseInBounds;

					if (maskingBox == null)
						mouseInBounds = box.Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains;
					else
						mouseInBounds = box.Intersect(maskingBox.Value).Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains;

					if (mouseInBounds)
					{
						Config[StateID] |= (uint)HudElementStates.IsMouseInBounds;
						HudMain.Cursor.TryCaptureHudSpace(cursorPos.Z, HudSpace.GetHudSpaceFunc);
					}
				}
			}

			/// <summary>
			/// Updates input for the element and attempts to capture the cursor if mouse input is enabled.
			/// Override HandleInput() for customization.
			/// </summary>
			protected sealed override void BeginInput()
			{
				Vector3 cursorPos = HudSpace.CursorPos;
				bool canUseCursor = (Config[StateID] & (uint)HudElementStates.CanUseCursor) > 0,
					canShareCursor = (Config[StateID] & (uint)HudElementStates.CanShareCursor) > 0;
				bool mouseInBounds = (Config[StateID] & (uint)HudElementStates.IsMouseInBounds) > 0;

				if (canUseCursor && mouseInBounds && !HudMain.Cursor.IsCaptured && HudMain.Cursor.IsCapturingSpace(HudSpace.GetHudSpaceFunc))
				{
					bool isMousedOver = mouseInBounds;

					if (isMousedOver)
						Config[StateID] |= (uint)HudElementStates.IsMousedOver;

					HandleInput(new Vector2(cursorPos.X, cursorPos.Y));

					if (!canShareCursor)
						HudMain.Cursor.Capture(DataHandle[0].Item3.Item1);
				}
				else
				{
					HandleInput(new Vector2(cursorPos.X, cursorPos.Y));
				}
			}

			/// <summary>
			/// Updates internal state and child alignment. Override Layout() for customization.
			/// </summary>
			protected sealed override void BeginLayout(bool _)
			{
				var parentFull = Parent as HudElementBase;
				HudSpace = Parent?.HudSpace;

				if (HudSpace != null)
					Config[StateID] |= (uint)HudElementStates.IsSpaceNodeReady;
				else
					Config[StateID] &= ~(uint)HudElementStates.IsSpaceNodeReady;

				if (parentFull != null)
				{
					Origin = parentFull.Position + OriginAlignment;
				}
				else
				{
					Origin = Vector2.Zero;
					Position = Offset;
					Padding = Padding;
					CachedSize = UnpaddedSize + Padding;
				}

				Layout();

				// Masking configuration
				if (parentFull != null && (parentFull.Config[StateID] & (uint)HudElementStates.IsMasked) > 0 &&
					(Config[StateID] & (uint)HudElementStates.CanIgnoreMasking) == 0
				)
					Config[StateID] |= (uint)HudElementStates.IsMasked;
				else
					Config[StateID] &= ~(uint)HudElementStates.IsMasked;

				if ((Config[StateID] & (uint)HudElementStates.IsMasking) > 0 ||
					(parentFull != null && (Config[StateID] & (uint)HudElementStates.IsSelectivelyMasked) > 0))
				{
					UpdateMasking();
				}
				else if ((Config[StateID] & (uint)HudElementStates.IsMasked) > 0)
					maskingBox = parentFull?.maskingBox;
				else
					maskingBox = null;

				// Check if masking results in no area
				bool isDisjoint = false;

				if ((Config[StateID] & (uint)HudElementStates.IsMasking) > 0 && maskingBox != null)
				{
					Vector2 halfSize = CachedSize * .5f;
					var bounds = new BoundingBox2(Position - halfSize, Position + halfSize);
					isDisjoint =
						(bounds.Max.X < maskingBox.Value.Min.X) ||
						(bounds.Min.X > maskingBox.Value.Max.X) ||
						(bounds.Max.Y < maskingBox.Value.Min.Y) ||
						(bounds.Min.Y > maskingBox.Value.Max.Y);
				}

				if (isDisjoint)
					Config[StateID] |= (uint)HudElementStates.IsDisjoint;
				else
					Config[StateID] &= ~(uint)HudElementStates.IsDisjoint;

				if (children.Count > 0)
					UpdateChildAlignment();
			}

			/// <summary>
			/// Updates cached values as well as parent and dim alignment.
			/// </summary>
			private void UpdateChildAlignment()
			{
				// Update size
				for (int i = 0; i < children.Count; i++)
				{
					var child = children[i] as HudElementBase;

					if (child != null)
						child.Config[StateID] |= (uint)HudElementStates.WasParentVisible;

					if (child != null && (child.Config[StateID] & (child.Config[VisMaskID])) == child.Config[VisMaskID])
					{
						child.Padding = child.Padding;

						Vector2 size = child.UnpaddedSize + child.Padding;
						DimAlignments sizeFlags = child.DimAlignment;

						if (sizeFlags != DimAlignments.None)
						{
							if ((sizeFlags & DimAlignments.IgnorePadding) == DimAlignments.IgnorePadding)
							{
								if ((sizeFlags & DimAlignments.Width) == DimAlignments.Width)
									size.X = CachedSize.X - Padding.X;

								if ((sizeFlags & DimAlignments.Height) == DimAlignments.Height)
									size.Y = CachedSize.Y - Padding.Y;
							}
							else
							{
								if ((sizeFlags & DimAlignments.Width) == DimAlignments.Width)
									size.X = CachedSize.X;

								if ((sizeFlags & DimAlignments.Height) == DimAlignments.Height)
									size.Y = CachedSize.Y;
							}

							child.UnpaddedSize = size - child.Padding;
						}

						child.CachedSize = size;
					}
				}

				// Update position
				for (int i = 0; i < children.Count; i++)
				{
					var child = children[i] as HudElementBase;

					if (child != null && (child.Config[StateID] & (child.Config[VisMaskID])) == child.Config[VisMaskID])
					{
						ParentAlignments originFlags = child.ParentAlignment;
						Vector2 delta = Vector2.Zero,
							max = (CachedSize + child.CachedSize) * .5f,
							min = -max;

						if ((originFlags & ParentAlignments.UsePadding) == ParentAlignments.UsePadding)
						{
							min += Padding * .5f;
							max -= Padding * .5f;
						}

						if ((originFlags & ParentAlignments.InnerV) == ParentAlignments.InnerV)
						{
							min.Y += child.CachedSize.Y;
							max.Y -= child.CachedSize.Y;
						}

						if ((originFlags & ParentAlignments.InnerH) == ParentAlignments.InnerH)
						{
							min.X += child.CachedSize.X;
							max.X -= child.CachedSize.X;
						}

						if ((originFlags & ParentAlignments.Bottom) == ParentAlignments.Bottom)
							delta.Y = min.Y;
						else if ((originFlags & ParentAlignments.Top) == ParentAlignments.Top)
							delta.Y = max.Y;

						if ((originFlags & ParentAlignments.Left) == ParentAlignments.Left)
							delta.X = min.X;
						else if ((originFlags & ParentAlignments.Right) == ParentAlignments.Right)
							delta.X = max.X;

						child.OriginAlignment = delta;
						child.Origin = Position + delta;
						child.Position = child.Origin + child.Offset;
					}
				}
			}

			/// <summary>
			/// Updates masking state and bounding boxes used to mask billboards
			/// </summary>
			private void UpdateMasking()
			{
				Config[StateID] |= (uint)HudElementStates.IsMasked;

				BoundingBox2? parentBox, box = null;
				var parentFull = Parent as HudElementBase;

				if ((Config[StateID] & (uint)HudElementStates.CanIgnoreMasking) > 0)
				{
					parentBox = null;
				}
				else if (parentFull != null && (Config[StateID] & (uint)HudElementStates.IsSelectivelyMasked) > 0)
				{
					Vector2 halfParent = .5f * parentFull.CachedSize;
					parentBox = new BoundingBox2(
						-halfParent + parentFull.Position,
						halfParent + parentFull.Position
					);

					if (parentFull.maskingBox != null)
						parentBox = parentBox.Value.Intersect(parentFull.maskingBox.Value);
				}
				else
					parentBox = parentFull?.maskingBox;

				if ((Config[StateID] & (uint)HudElementStates.IsMasking) > 0)
				{
					Vector2 halfSize = .5f * CachedSize;
					box = new BoundingBox2(
						-halfSize + Position,
						halfSize + Position
					);
				}

				if (parentBox != null && box != null)
					box = box.Value.Intersect(parentBox.Value);
				else if (box == null)
					box = parentBox;

				maskingBox = box;
			}

			/// <summary>
			/// Internal debugging method
			/// </summary>
			protected override object GetOrSetApiMember(object data, int memberEnum)
			{
				switch ((HudElementAccessors)memberEnum)
				{
					case HudElementAccessors.Position:
						return Position;
					case HudElementAccessors.Size:
						return Size;
				}

				return base.GetOrSetApiMember(data, memberEnum);
			}
		}
	}
}