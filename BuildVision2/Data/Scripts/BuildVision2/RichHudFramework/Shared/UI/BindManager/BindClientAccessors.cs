using System;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Flags determining which game inputs are blocked (blacklisted) by the framework.
	/// <para>Useful for modifying or disabling normal game input behavior while UI elements are active.</para>
	/// </summary>
	[Flags]
	public enum SeBlacklistModes : int
	{
		/// <summary>
		/// Default: No inputs are blocked. Game functions normally.
		/// </summary>
		None = 0,

		/// <summary>
		/// Disables mouse button clicks (LMB, RMB, MMB, etc.) from reaching the game.
		/// </summary>
		Mouse = 1 << 0,

		/// <summary>
		/// Disables all blacklist-able keys (Keyboard and Mouse). 
		/// <para>Note: Not every Space Engineers bind can be disabled via the API.</para>
		/// </summary>
		AllKeys = 1 << 1 | Mouse,

		/// <summary>
		/// Disables camera rotation (mouse look). Does not disable looking while holding Alt.
		/// </summary>
		CameraRot = 1 << 2,

		/// <summary>
		/// Disables both mouse button clicks and camera rotation.
		/// </summary>
		MouseAndCam = Mouse | CameraRot,

		/// <summary>
		/// Disables all standard key binds and camera rotation.
		/// </summary>
		Full = AllKeys | CameraRot,

		/// <summary>
		/// Intercepts chat input, preventing chat messages from sending.
		/// </summary>
		Chat = 1 << 3,

		/// <summary>
		/// Comprehensive blocking: Disables all keys, camera rotation, and intercepts chat.
		/// </summary>
		FullWithChat = Full | Chat
	}

	/// <summary>
	/// Internal bind client API enums
	/// </summary>
	/// <exclude/>
	public enum BindClientAccessors : int
    {
        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetOrCreateGroup = 1,

        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetBindGroup = 2,

        /// <summary>
        /// in: IReadOnlyList{string}, out: int[]
        /// </summary>
        GetComboIndices = 3,

        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetControlByName = 4,

        /// <summary>
        /// void
        /// </summary>
        ClearBindGroups = 5,

        /// <summary>
        /// void
        /// </summary>
        Unload = 6,

        /// <summary>
        /// in/out: SeBlacklistModes
        /// </summary>
        RequestBlacklistMode = 7,

        /// <summary>
        /// out: bool
        /// </summary>
        IsChatOpen = 8,

        /// <summary>
        /// in: int, out: string
        /// </summary>
        GetControlName = 9,

        /// <summary>
        /// in: IReadOnlyList{int}, out: string[]
        /// </summary>
        GetControlNames = 10,
    }
}