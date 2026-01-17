using GeoTimeZone;
using Wordiny.Api.Helpers;
using Wordiny.Api.Models;
using Wordiny.Api.Resources;
using Wordiny.DataAccess.Models;

namespace Wordiny.Api.Services.Handlers;

public interface IMessageHandler
{
    Task HandleAsync(Message message, CancellationToken token = default);
}

public class MessageHandler : IMessageHandler
{
    private readonly ILogger<MessageHandler> _logger;
    private readonly IUserService _userService;
    private readonly ITelegramApiService _telegramApiService;
    private readonly IPhraseService _phraseService;

    public MessageHandler(
        ILogger<MessageHandler> logger,
        IUserService userService,
        ITelegramApiService telegramApiService,
        IPhraseService phraseService)
    {
        _logger = logger;
        _userService = userService;
        _telegramApiService = telegramApiService;
        _phraseService = phraseService;
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
                    await _telegramApiService.SendMessageAsync(userId, BotMessages.SetupTimeZone, token: token);

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
                    if (message.Location is null)
                    {
                        await _telegramApiService.SendMessageAsync(
                            userId,
                            BotMessages.SetupTimeZone_InvalidLocation,
                            token: token);

                        break;
                    }

                    var tzResult = TimeZoneLookup.GetTimeZone(message.Location.Latitude, message.Location.Longitude);
                    if (tzResult is null || tzResult.Result is null)
                    {
                        await _telegramApiService.SendMessageAsync(
                            userId,
                            BotMessages.SetupTimeZone_Failed,
                            token: token);

                        break;
                    }

                    var ianaTzId = tzResult.Result;
                    
                    await _userService.SetTimeZoneAsync(userId, ianaTzId, token);
                    await _userService.SetInputStateAsync(userId, UserInputState.ConfirmTimeZone, token);

                    await _telegramApiService.SendMessageAsync(userId, string.Format(BotMessages.ConfirmTimeZone, ianaTzId), token: token);

                    break;
                }
            case UserInputState.ConfirmTimeZone:
                {
                    switch (message.Text.ToLower())
                    {
                        case "да":
                            await _userService.SetInputStateAsync(userId, UserInputState.SetFrequence, token);
                            await _telegramApiService.SendMessageAsync(userId, BotMessages.SetupFrequency, token: token);

                            break;
                        case "нет":
                            await _userService.SetInputStateAsync(userId, UserInputState.SetTimeZone, token);
                            await _telegramApiService.SendMessageAsync(userId, BotMessages.SetupTimeZone, token: token);

                            break;
                        default:
                            {
                                await _telegramApiService.SendMessageAsync(userId, BotMessages.ConfirmTimeZone_InvalidInput, token: token);
                                break;
                            }
                    }

                    break;
                }
            case UserInputState.SetFrequence:
                {
                    if (!Enum.TryParse(typeof(RepeatFrequencyInDay), message.Text, true, out var frequencyObj)
                        || frequencyObj is not RepeatFrequencyInDay frequencyInDay
                        || !Enum.IsDefined(frequencyInDay))
                    {
                        await _telegramApiService.SendMessageAsync(
                            userId,
                            BotMessages.SetupFrequency_InvalidInput,
                            token: token);

                        break;
                    }

                    await _userService.SetRepeatFrequencyInDayAsync(userId, frequencyInDay, token);
                    await _telegramApiService.SendMessageAsync(userId, BotMessages.SetupFinished, token: token);
                    await _userService.SetInputStateAsync(userId, UserInputState.AwaitingPhraseAdding, token);

                    break;
                }
            case UserInputState.AwaitingPhraseAdding:
                {
                    if (string.IsNullOrWhiteSpace(message.Text))
                    {
                        _logger.LogError("User {userId} send a empty phrase", userId);
                        break;
                    }

                    var addedPhrase = await _phraseService.AddNewPhraseAsync(userId, message.Text, token);
                    await _userService.SetInputStateAsync(userId, UserInputState.AwaitingPhraseTranslation, token);
                    await _telegramApiService.SendMessageAsync(
                        userId, 
                        string.Format(BotMessages.AwaitingWordTranslation, message.Text),
                        token: token);

                    break;
                }
            case UserInputState.AwaitingPhraseTranslation:
                {
                    if (string.IsNullOrWhiteSpace(message.Text))
                    {
                        _logger.LogError("User {userId} send a empty translation", userId);
                        break;
                    }

                    if (message.Text == ReplyActions.CANCEL_INPUT)
                    {
                        await _userService.SetInputStateAsync(userId, UserInputState.AwaitingPhraseAdding, token);
                        await _phraseService.RemoveLastPhraseAsync(userId, token);
                        await _telegramApiService.SendMessageAsync(userId, "Ввод отменён", token: token);

                        break;
                    }

                    var phrase = await _phraseService.AddPhraseTranslationAsync(userId, message.Text, token);
                    await _userService.SetInputStateAsync(userId, UserInputState.AwaitingPhraseAdding, token);

                    var responseMessage = string.Format(BotMessages.AwaitingWordTranslation_Complete, phrase.NativeText, phrase.TranslationText);

                    await _telegramApiService.SendMessageAsync(
                        userId, 
                        responseMessage,
                        [new("Удалить 🗑️", CallbackCommands.DeletePhrase(phrase.Id))],
                        token: token);

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
