using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudNodeHookData = VRage.MyTuple<
	System.Func<object, int, object>, // 1 -  GetOrSetApiMemberFunc
	System.Action, // 2 - InputDepthAction
	System.Action, // 3 - InputAction
	System.Action, // 4 - SizingAction
	System.Action<bool>, // 5 - LayoutAction
	System.Action // 6 - DrawAction
>;
using HudSpaceOriginFunc = System.Func<VRageMath.Vector3D>;

namespace RichHudFramework
{
	using HudNodeData = MyTuple<
		uint[], // 1 - Config { 1.0 - State, 1.1 - NodeVisibleMask, 1.2 - NodeInputMask, 1.3 - zOffset, 1.4 - zOffsetInner, 1.5 - fullZOffset }
		HudSpaceOriginFunc[],  // 2 - GetNodeOriginFunc
		HudNodeHookData, // 3 - Main hooks
		object, // 4 - Parent as HudNodeDataHandle
		List<object>, // 5 - Children as IReadOnlyList<HudNodeDataHandle>
		object // 6 - Unused
	>;

	namespace UI
	{
		using static RichHudFramework.UI.NodeConfigIndices;

		// Read-only length-1 array containing raw UI node data
		using HudNodeDataHandle = IReadOnlyList<HudNodeData>;

		/// <summary>
		/// Internal indices for accessing UI node config
		/// </summary>
		/// <exclude/>
		public static class NodeConfigIndices
		{
			/// <summary>
			/// type: HudElementStates
			/// </summary>
			public const int StateID = 0;

			/// <summary>
			/// type: HudElementStates
			/// </summary>
			public const int VisMaskID = 1;

			/// <summary>
			/// type: HudElementStates
			/// </summary>
			public const int InputMaskID = 2;

			/// <summary>
			/// Stores publicly exposed sorting offset local to a UI node.
			/// type: sbyte
			/// </summary>
			public const int ZOffsetID = 3;

			/// <summary>
			/// Stores private inner offset used for window layering local to the UI node.
			/// type: byte
			/// </summary>
			public const int ZOffsetInnerID = 4;

			/// <summary>
			/// Stores private combined inner and outer offsets, including total offsets of all preceeding
			/// parent nodes.
			/// type: ushort
			/// </summary>
			public const int FullZOffsetID = 5;

			/// <summary>
			/// Stores the frame number of the last time the node was updated.
			/// type: int
			/// </summary>
			public const int FrameNumberID = 6;

			public const int ConfigLength = 7;
		}

		public abstract partial class HudParentBase
		{
			protected static partial class ParentUtils
			{
				/// <summary>
				/// Wrapper around a shared reference to a UI element's shared tree data.
				/// Used internally for documentationn and compile-time validation. Do not use.
				/// </summary>
				/// <exclude/>
				private struct LinkedHudNode
				{
					/// <summary>
					/// Parent object of the node
					/// </summary>
					public LinkedHudNode Parent => new LinkedHudNode { dataRef = (dataRef[0].Item4 as HudNodeDataHandle) };

					/// <summary>
					/// Internal state tracking flags
					/// </summary>
					public HudElementStates State => (HudElementStates)dataRef[0].Item1[StateID];

					/// <summary>
					/// Internal state mask for determining visibility
					/// </summary>
					public HudElementStates NodeVisibleMask => (HudElementStates)dataRef[0].Item1[VisMaskID];

					/// <summary>
					/// Internal state mask for determining whether input updates are enabled
					/// </summary>
					public HudElementStates NodeInputMask => (HudElementStates)dataRef[0].Item1[InputMaskID];

					/// <summary>
					/// Determines whether the UI element will be drawn in the Back, Mid or Foreground
					/// </summary>
					public sbyte ZOffset => (sbyte)dataRef[0].Item1[ZOffsetID];

					/// <summary>
					/// Used for input focus and window sorting
					/// </summary>
					public byte ZOffsetInner => (byte)dataRef[0].Item1[ZOffsetInnerID];

					/// <summary>
					/// Combined offset used for final sorting
					/// </summary>
					public ushort FullZOffset => (ushort)dataRef[0].Item1[FullZOffsetID];

					/// <summary>
					/// Used to check whether the cursor is moused over the element and whether its being
					/// obstructed by another element.
					/// </summary>
					public Action InputDepthCallback => dataRef[0].Item3.Item2;

					/// <summary>
					/// Updates the input of this UI element. Invocation order affected by z-Offset and depth sorting.
					/// Executes last, after Draw.
					/// </summary>
					public Action HandleInputCallback => dataRef[0].Item3.Item3;

					/// <summary>
					/// Updates the sizing of the element. Executes before layout in bottom-up order, before layout.
					/// </summary>
					public Action UpdateSizeCallback => dataRef[0].Item3.Item4;

					/// <summary>
					/// Updates the internal layout of the UI element. Executes after sizing in top-down order, before 
					/// input and draw. Not affected by depth or z-Offset sorting.
					/// </summary>
					public Action<bool> LayoutCallback => dataRef[0].Item3.Item5;

					/// <summary>
					/// Used to immediately draw billboards. Invocation order affected by z-Offset and depth sorting.
					/// Executes after Layout and before HandleInput.
					/// </summary>
					public Action DrawCallback => dataRef[0].Item3.Item6;

					/// <summary>
					/// Delegate for getting HUD space translation in world space
					/// </summary>
					public HudSpaceOriginFunc GetHudNodeOriginFunc => dataRef[0].Item2[0];

					/// <summary>
					/// Debugging info delegate
					/// </summary>
					public ApiMemberAccessor GetOrSetMemberFunc => dataRef[0].Item3.Item1;

					/// <summary>
					/// Raw RHF API data
					/// </summary>
					public HudNodeDataHandle dataRef;
				}
			}
		}
	}
}