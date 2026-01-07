using Telegram.Bot;
using Telegram.Bot.Extensions;
using Wordiny.Api.Exceptions;

namespace Wordiny.Api.Services;

public interface ITelegramApiService
{
    public Task<Telegram.Bot.Types.Message> SendMessageAsync(
        long userId, 
        string message,
        CancellationToken token = default);
}

public class TelegramApiService : ITelegramApiService
{
    private readonly ITelegramBotClient _botClient;

    public TelegramApiService([FromKeyedServices("Wordiny")]ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task<Telegram.Bot.Types.Message> SendMessageAsync(
        long userId, 
        string message,
        CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        try
        {
            var sentMessages = await _botClient.SendHtml(userId, message);

            return sentMessages.First();
        }
        catch (Exception ex)
        {
            // https://stackoverflow.com/questions/35263618/how-can-i-detect-whether-a-user-deletes-the-telegram-bot-chat
            if (ex.Message.Contains("blocked"))
            {
                throw new UserUndeliverableException(userId, isDeleted: false, ex.Message);
            }
            else if (ex.Message.Contains("deactivated"))
            {
                throw new UserUndeliverableException(userId, isDeleted: true, ex.Message);
            }

            throw new TelegramSendMessageException(userId, ex.Message);
        }
    }
}
