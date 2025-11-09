namespace Wordiny.Api.Helpers;

public static class BotCommands
{
    public static bool IsBotComamand(string message) => message.StartsWith('/');

    public const string START = "/start";
}
