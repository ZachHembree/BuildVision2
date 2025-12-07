namespace RichHudFramework
{
	/// <summary>
	/// API version IDs for extensions/revisions to Master-Client interface
	/// </summary>
	/// <exclude/>
	public enum APIVersionTable : int
	{
		/// <summary>
		/// Version 1.0+ baseline
		/// </summary>
		Version1Base = 7,
		InputModeSupport = 8,
		TextRefMatrixDrawSupport = 9,
		BBUtilsSupport = 10,
		BBUtils3DSupport = 11,
		BindAliasAndAnalogSupport = 12,
		HudNodeHandleSupport = 13,

		Latest = HudNodeHandleSupport
	}
}