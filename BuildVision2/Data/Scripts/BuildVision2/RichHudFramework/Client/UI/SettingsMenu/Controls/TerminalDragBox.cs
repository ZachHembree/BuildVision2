using VRageMath;

namespace RichHudFramework.UI.Client
{
	/// <summary>
	/// Internal API member accessor indices
	/// </summary>
	/// <exclude/>
	public enum DragBoxAccessors : int
	{
		BoxSize = 16,
		AlignToEdge = 17,
	}

	/// <summary>
	/// A control allowing the user to visually select a 2D screen position (Vector2) for a <see cref="ControlTile"/>.
	/// <para>Spawns a temporary draggable window when interacted with. Useful for configuring HUD element positions.</para>
	/// </summary>
	public class TerminalDragBox : TerminalValue<Vector2>
	{
		/// <summary>
		/// The size of the draggable window spawned by this control.
		/// </summary>
		public Vector2 BoxSize
		{
			get { return (Vector2)GetOrSetMember(null, (int)DragBoxAccessors.BoxSize); }
			set { GetOrSetMember(value, (int)DragBoxAccessors.BoxSize); }
		}

		/// <summary>
		/// If true, the window aligns itself to the nearest screen edge.
		/// </summary>
		public bool AlignToEdge
		{
			get { return (bool)GetOrSetMember(null, (int)DragBoxAccessors.AlignToEdge); }
			set { GetOrSetMember(value, (int)DragBoxAccessors.AlignToEdge); }
		}

		public TerminalDragBox() : base(MenuControls.DragBox)
		{ }
	}
}