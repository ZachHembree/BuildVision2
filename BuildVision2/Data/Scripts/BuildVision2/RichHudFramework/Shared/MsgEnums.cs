namespace RichHudFramework
{
    /// <summary>
    /// API registration state enums
    /// </summary>
    /// <exclude/>
    public enum MsgTypes : int
    {
        RegistrationRequest = 1,
        RegistrationSuccessful = 2,
        RegistrationFailed = 3,
    }

	/// <summary>
	/// API submodule type enums
	/// </summary>
	/// <exclude/>
	public enum ApiModuleTypes : int
    {
        BindManager = 1,
        HudMain = 2,
        FontManager = 3,
        SettingsMenu = 4,
        BillBoardUtils = 5
    }

	/// <summary>
	/// Main client accessor properties
	/// </summary>
	/// <exclude/>
	public enum ClientDataAccessors : int
	{
		GetVersionID = 1,
		GetSubtype = 2,
		ReportException = 3,
		GetIsPausedFunc = 4
	}

	/// <summary>
	/// Client configuration enums
	/// </summary>
	/// <exclude/>
	public enum ClientSubtypes : int
    {
        Full = 1,
        NoLib = 2,
        Terminal = 3,
        FontManager = 4,
        BindManager = 5
    }
}