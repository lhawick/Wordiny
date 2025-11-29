using Wordiny.Api.Helpers;
using Wordiny.Api.Models;
using Wordiny.Api.Resources;
using Wordiny.DataAccess.Models;

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

        var userSettingStep = await _userSettingsService.GetSettingStepAsync(message.UserId, token);
        if (userSettingStep != SettingsStep.Setupped)
        {
            await HandleUserSetting(message, token);
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
                        }

                        break;
                    }

                    var newUser = await _userService.AddUserAsync(userId, token);
                    if (newUser is null)
                    {
                        _logger.LogWarning("User {userId} is already exist", message.UserId);
                    }

                    await _telegramApiService.SendMessageAsync(userId, BotMessages.Welcome, token: token);
                    await _userSettingsService.StartSettingAsync(userId, token);
                    await _telegramApiService.SendMessageAsync(userId, BotMessages.SetupTimeZone, useCache: true, token);

                    break;
                }
            default:
                {
                    _logger.LogError("No handlers for bot command: {botCommand}", message.Text);
                    break;
                }
        }
    }

    private async Task HandleUserSetting(Message message, CancellationToken token = default)
    {
        var userSettingStep = await _userSettingsService.GetSettingStepAsync(message.UserId, token);
        if (userSettingStep == SettingsStep.Setupped)
        {
            return;
        }

        switch (userSettingStep)
        {
            case SettingsStep.SetTimeZone:
                {
                    if (!short.TryParse(message.Text, out var timeZone))
                    {
                        await _telegramApiService.SendMessageAsync(
                            message.UserId,
                            "Часовой пояс указан неправильно. Пожалуйста, отправьте только число",
                            token: token);

                        break;
                    }
                    
                    await _userSettingsService.SetupTimeZoneAsync(message.UserId, timeZone, token);
                    // TODO: отправляем клаву

                    break;
                }
            case SettingsStep.SetFrequence:
                {
                    if (!Enum.TryParse(typeof(RepeatFrequencyInDay), message.Text, true, out var frequencyObj)
                        || frequencyObj is not RepeatFrequencyInDay frequencyInDay)
                    {
                        await _telegramApiService.SendMessageAsync(
                            message.UserId,
                            "Частота отправки указана неверно. Пожалуйста, укажите значение с клавиатуры",
                            token: token);

                        // TODO: тут скорее всего тоже клаву надо

                        break;
                    }

                    await _userSettingsService.SetupRepeatFrequencyInDayAsync(message.UserId, frequencyInDay, token);

                    break;
                }
            case SettingsStep.NoSettings:
                _logger.LogWarning("Try handle setup settings while user {userId} has not active setting process", message.UserId);
                return;

            default:
                throw new InvalidOperationException($"No handlers for setting step {userSettingStep}");
        }
    }
}
