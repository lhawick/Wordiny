using Telegram.Bot;
using Wordiny.Api.Resources;

namespace Wordiny.Api.Services;

public interface IUserSettingsService
{
    public Task StartSetupAsync(long userId, CancellationToken token = default);
}

public class UserSettingsService : IUserSettingsService
{
    private readonly ITelegramBotClient _telegramBotClient;

    public UserSettingsService(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public async Task StartSetupAsync(long userId, CancellationToken token = default)
    {


        await _telegramBotClient.SendMessage(
            userId, 
            BotMessages.SetupTimeZone, 
            Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
            cancellationToken: token);
    }
}
