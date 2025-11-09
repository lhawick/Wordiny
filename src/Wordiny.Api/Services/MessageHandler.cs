using Telegram.Bot;
using Wordiny.Api.Helpers;
using Wordiny.Api.Models;

namespace Wordiny.Api.Services;

public interface IMessageHandler
{
    Task HandleAsync(Message message, CancellationToken token = default);
}

public class MessageHandler : IMessageHandler
{
    private readonly ILogger<MessageHandler> _logger;
    private readonly IUserService _userService;
    private readonly IUserSettingsService _userSettingsService;
    private readonly ITelegramApiService _telegramApiService;

    public MessageHandler(
        ILogger<MessageHandler> logger,
        IUserService userService,
        IUserSettingsService userSettingsService,
        ITelegramApiService telegramApiService)
    {
        _logger = logger;
        _userService = userService;
        _userSettingsService = userSettingsService;
        _telegramApiService = telegramApiService;
    }

    public async Task HandleAsync(Message message, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        if (BotCommands.IsBotComamand(message.Text))
        {
            await HandleBotCommandAsync(message, token);
            return;
        }
    }

    private async Task HandleBotCommandAsync(Message message, CancellationToken token = default)
    {
        switch (message.Text)
        {
            case BotCommands.START:
                {
                    var isUserExist = await _userService.IsUserExistAsync(message.UserId, token);
                    if (isUserExist)
                    {
                        break;
                    }

                    await _userService.AddUserAsync(message.UserId, token);

                    await _telegramApiService.SendMessageAsync(message.UserId);

                    await _userSettingsService.StartSetupAsync(message.UserId, token);

                    break;
                }
            default:
                {
                    _logger.LogError("No handlers for bot command: {botCommand}", message.Text);
                    break;
                }
        }
    }
}
