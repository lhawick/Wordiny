using System.Reflection;
using Wordiny.Api.Helpers;
using Wordiny.Api.Models;
using Wordiny.Api.Resources;
using Wordiny.DataAccess.Models;

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

        var callbackType =callbackData[0];

        switch (callbackType)
        {
            case CallbackCommands.SPECIFY_CITY:
                {
                    if (callbackData.Length < 2)
                    {
                        throw new InvalidOperationException(
                            $"Callback {nameof(CallbackCommands.SPECIFY_CITY)} has invalid data length");
                    }

                    var timeZone = callbackData[1];

                    if (string.IsNullOrWhiteSpace(timeZone))
                    {
                        throw new InvalidOperationException(
                            $"Callback {nameof(CallbackCommands.SPECIFY_CITY)} has not valid timeZone in data");
                    }

                    await _userService.SetTimeZoneAsync(userId, timeZone, token);
                    await _userService.SetInputStateAsync(userId, UserInputState.ConfirmTimeZone, token);

                    await _telegramApiService.SendMessageAsync(
                        userId, 
                        string.Format(BotMessages.ConfirmTimeZone, timeZone), 
                        token: token);

                    break;
                }
            default:
                {
                    await _telegramApiService.SendMessageAsync(userId, BotMessages.UnknowCallbackType, token: token);
                    break;
                }
        }
    }
}
