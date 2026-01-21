namespace Wordiny.Api.Helpers;

public static class CallbackCommands
{
    public const string DELIMETER = ":";


    public const string DELETE_PHRASE = "DeletePhrase";
    public static string DeletePhrase(long phraseId) => DELETE_PHRASE + DELIMETER + phraseId;

    public const string CANCEL_PHRASE_INPUT = "CancelPhraseInput";
    public static string CancelPhraseInput(long phraseId) => CANCEL_PHRASE_INPUT + DELIMETER + phraseId;
}
