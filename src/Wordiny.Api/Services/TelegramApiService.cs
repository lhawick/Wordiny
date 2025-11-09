using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Wordiny.Api.Services;

public interface ITelegramApiService
{
    public Task SendMessageAsync(long userId, string message, bool useCache = true, CancellationToken token = default);
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

    public async Task SendMessageAsync(long userId, string message, bool useCache = true, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        try
        {
            // Экранируем определённые символы для MarkdownV2
            var screeningMessage = GetScreeningMessage(message, useCache);

            await _botClient.SendMessage(userId, screeningMessage, ParseMode.MarkdownV2, cancellationToken: token);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send message to user {userId}: {errorMessage}", userId, ex.Message);

            // https://stackoverflow.com/questions/35263618/how-can-i-detect-whether-a-user-deletes-the-telegram-bot-chat
            if (ex.Message.Contains("blocked"))
            {
                await _userService.DisabledUser(userId, token);
            }
            else if (ex.Message.Contains("deactivated"))
            {
                await _userService.DeleteUserAsync(userId, token);
            }
        }
    }

    private static readonly string[] _screeningChars =
    [
        "\\", "_", "*", "[", "]", "(", ")", "~", "`",
        ">", "#", "+", "-", "=", "|", "{", "}", ".", "!"
    ];

    private string GetScreeningMessage(string message, bool useCache)
    {
        var cacheKey = GetMd5Hash(message);
        if (useCache && _memoryCache.TryGetValue<string>(cacheKey, out var screeningMessage) 
            && screeningMessage is not null)
        {
            return screeningMessage;
        }

        var stringBuilder = new StringBuilder(message);

        foreach (var screeningChar in _screeningChars)
        {
            stringBuilder.Replace(screeningChar, "\\" + screeningChar);
        }

        screeningMessage = stringBuilder.ToString();

        if (useCache)
        {
            _memoryCache.Set(cacheKey, screeningMessage, TimeSpan.FromMinutes(30));
        }

        return screeningMessage;
    }

    private static string GetMd5Hash(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var hashBytes = MD5.HashData(messageBytes);

        var hashAsString = new StringBuilder();
        foreach (var @byte in hashBytes)
        {
            hashAsString.Append(@byte.ToString("x2"));
        }

        return hashAsString.ToString();
    }
}
