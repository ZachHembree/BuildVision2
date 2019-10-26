namespace DarkHelmet
{
    public enum MsgTypes : int
    {
        RegistrationRequest = 1,
        RegistrationSuccessful = 2,
        RegistrationFailed = 3,
        DataRequest = 4
    }

    public enum ApiComponentTypes : int
    {
        BindManager = 0,
        HudMain = 1
    }
}