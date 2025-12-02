using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types.Enums;
using Wordiny.Api.Exceptions;

namespace Wordiny.Api.Services;

public interface ITelegramApiService
{
    public Task<Telegram.Bot.Types.Message> SendMessageAsync(
        long userId, 
        string message, 
        bool useCache = true, 
        CancellationToken token = default);
}

public class TelegramApiService : ITelegramApiService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly ILogger<TelegramApiService> _logger;
    private readonly IMemoryCache _memoryCache;

    public TelegramApiService(
        ITelegramBotClient botClient,
        IUserService userService,
        ILogger<TelegramApiService> logger,
        IMemoryCache memoryCache)
    {
        _botClient = botClient;
        _userService = userService;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<Telegram.Bot.Types.Message> SendMessageAsync(
        long userId, 
        string message, 
        bool useCache = true, 
        CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        try
        {
            return await _botClient.SendHtml(userId, message);
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
