namespace Wordiny.Api.Helpers;

public static class CallbackCommands
{
    public const string DELIMETER = ":";

    public const string DELETE_PHRASE = "DeletePhrase";
    public static string DeletePhrase(long phraseId) => DELETE_PHRASE + DELIMETER + phraseId;

    public const string SPECIFY_CITY = "SpecityCity";
    public static string SpecifyCity(string timeZone) => SPECIFY_CITY + DELIMETER + timeZone;
}
