using Wordiny.Api.Helpers;
using Wordiny.Api.Models;

namespace Wordiny.Api.Services.Handlers;

public interface ICallbackQueryHandler
{
    Task HandleAsync(CallbackQuery callback, CancellationToken token = default);
}

public class CallbackQueryHandler : ICallbackQueryHandler
{
    private readonly ILogger<CallbackQueryHandler> _logger;
    private readonly IPhraseService _phraseService;
    private readonly ITelegramApiService _telegramApiService;
    private readonly IUserService _userService;

    public CallbackQueryHandler(
        ILogger<CallbackQueryHandler> logger,
        IPhraseService phraseService,
        ITelegramApiService telegramApiService,
        IUserService userService)
    {
        _logger = logger;
        _phraseService = phraseService;
        _telegramApiService = telegramApiService;
        _userService = userService;
    }

    public async Task HandleAsync(CallbackQuery callback, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));

        var userId = callback.UserId;

        if (string.IsNullOrWhiteSpace(callback.Data))
        {
            _logger.LogError("No callback data (userId: {userId})", userId);

            return;
        }

        var callbackData = callback.Data.Split(CallbackCommands.DELIMETER);

        if (callbackData.Length == 0)
        {
            _logger.LogError("Invalid callback data (userId: {userId}): {callbackData}", userId, callback.Data);

            return;
        }

        switch (callbackData[0])
        {
            case CallbackCommands.DELETE_PHRASE:
                {
                    if (!long.TryParse(callbackData[1], out var phraseId))
                    {
                        throw new InvalidOperationException("Failed to parse phraseId from callback data");
                    }

                    await _phraseService.RemovePhraseAsync(phraseId, token);
                    await _telegramApiService.SendMessageAsync(userId, "Успешно удалено", token: token);

                    break;
                }
            default:
                {
                    _logger.LogError("Unknow callback command: {callbackCommand}", callbackData[0]);
                    break;
                }
        }
    }
}
