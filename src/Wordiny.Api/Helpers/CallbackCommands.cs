namespace Wordiny.Api.Helpers;

public static class CallbackCommands
{
    public const string DELIMETER = ":";


    public const string DELETE_PHRASE = "DeletePhrase";
    public static string DeletePhrase(long phraseId) => DELETE_PHRASE + DELIMETER + phraseId;
}
