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
    private readonly ITelegramApiService _telegramApiService;

    public MessageHandler(
        ILogger<MessageHandler> logger,
        IUserService userService,
        ITelegramApiService telegramApiService)
    {
        _logger = logger;
        _userService = userService;
        _telegramApiService = telegramApiService;
    }

    public async Task HandleAsync(Message message, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        if (BotCommands.IsBotComamand(message.Text))
        {
            await HandleBotCommandAsync(message, token);
        }
        else
        {
            await HandleUserInputAsync(message, token);
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
                        _logger.LogWarning("User {userId} is already exist", userId);
                    }

                    await _telegramApiService.SendMessageAsync(userId, BotMessages.Welcome, token: token);
                    await _userService.SetInputStateAsync(userId, UserInputState.SetTimeZone, token);
                    await _telegramApiService.SendMessageAsync(userId, BotMessages.SetupTimeZone, token);

                    break;
                }
            default:
                {
                    _logger.LogError("No handlers for bot command: {botCommand}", message.Text);
                    break;
                }
        }
    }

    private async Task HandleUserInputAsync(Message message, CancellationToken token = default)
    {
        var userId = message.UserId;
        var userInputState = await _userService.GetInputStateAsync(userId, token);

        switch (userInputState)
        {
            case UserInputState.SetTimeZone:
                {
                    if (!short.TryParse(message.Text, out var timeZone))
                    {
                        await _telegramApiService.SendMessageAsync(
                            userId,
                            "Часовой пояс указан неправильно. Пожалуйста, отправьте только число",
                            token: token);

                        break;
                    }
                    
                    await _userService.SetTimeZoneAsync(userId, timeZone, token);
                    await _userService.SetInputStateAsync(userId, UserInputState.SetFrequence, token);
                    // TODO: отправляем клаву

                    break;
                }
            case UserInputState.SetFrequence:
                {
                    if (!Enum.TryParse(typeof(RepeatFrequencyInDay), message.Text, true, out var frequencyObj)
                        || frequencyObj is not RepeatFrequencyInDay frequencyInDay)
                    {
                        await _telegramApiService.SendMessageAsync(
                            userId,
                            "Частота отправки указана неверно. Пожалуйста, укажите значение с клавиатуры",
                            token: token);

                        // TODO: тут скорее всего тоже клаву надо

                        break;
                    }

                    await _userService.SetRepeatFrequencyInDayAsync(userId, frequencyInDay, token);
                    await _userService.SetInputStateAsync(userId, UserInputState.AwaitingWordAdding, token);

                    break;
                }
            case UserInputState.None:
                _logger.LogWarning("Try handle setup settings while user {userId} has not active setting process", userId);
                return;

            default:
                throw new InvalidOperationException($"No handlers for user input state {userInputState}");
        }
    }
}
