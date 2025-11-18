using Telegram.Bot;
using Wordiny.Api.Helpers;
using Wordiny.Api.Models;
using Wordiny.Api.Resources;

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
        var userId = message.UserId;

        switch (message.Text)
        {
            case BotCommands.START:
                {
                    var user = await _userService.GetUserAsync(userId, token);
                    if (user != null)
                    {
                        if (user.IsDisabled)
                        {
                            await _userService.EnableUserAsync(userId, token);
                            await _telegramApiService.SendMessageAsync(userId, BotMessages.UserReturn, token: token);

                            break;
                        }
                    }

                    await _userService.AddUserAsync(userId, token);
                    await _telegramApiService.SendMessageAsync(userId, BotMessages.Welcome, token: token);
                    await _userSettingsService.StartSetupAsync(userId, token);

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
