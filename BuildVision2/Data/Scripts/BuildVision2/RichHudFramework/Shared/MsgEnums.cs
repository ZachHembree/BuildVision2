namespace RichHudFramework
{
    public enum MsgTypes : int
    {
        RegistrationRequest = 1,
        RegistrationSuccessful = 2,
        RegistrationFailed = 3,
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
        GetGeneralAccessor = 1,
    }
}