using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotLogger;

public class TelegramBotLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, TelegramBotLogger> _loggers = new();
    private readonly ChatId[] _usersToSend;
    private readonly ITelegramBotClient _telegramBotClient;

    public TelegramBotLoggerProvider(ChatId[] usersToSend, ITelegramBotClient telegramBotClient)
    {
        _usersToSend = usersToSend;
        _telegramBotClient = telegramBotClient;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new TelegramBotLogger(_telegramBotClient, _usersToSend, name));
    }

    public void Dispose()
    {
        foreach (var logger in _loggers.Values)
        {
            logger.Dispose();
        }

        _loggers.Clear();
    }
}
