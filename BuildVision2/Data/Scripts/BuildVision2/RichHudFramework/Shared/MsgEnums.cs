namespace RichHudFramework
{
    public enum MsgTypes : int
    {
        RegistrationRequest = 1,
        RegistrationSuccessful = 2,
        RegistrationFailed = 3,
        GetHudUpdateAccessor = 4
    }

    public enum ApiModuleTypes : int
    {
        BindManager = 1,
        HudMain = 2,
        FontManager = 3,
        SettingsMenu = 4,
    }

    public enum ClientDataAccessors : int
    {   
        /// <summary>
        /// out: int
        /// </summary>
        MinVersionID = 1,

        /// <summary>
        /// out: int
        /// </summary>
        MasterVersionID = 2,

        /// <summary>
        /// in/out: bool
        /// </summary>
        EnableCursor = 3,

        /// <summary>
        /// in/out: bool
        /// </summary>
        RefreshDrawList = 4,
    }
}