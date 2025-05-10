using Microsoft.Extensions.Options;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Wordiny.Api.Config;

namespace Wordiny.Api.Services;

public class ConfigureWebhookService : IHostedService
{
    private readonly BotConfig _botConfig;
    private readonly ITelegramBotClient _telegramBotClient;

    public ConfigureWebhookService(IOptions<BotConfig> botConfig, ITelegramBotClient telegramBotСlient)
    {
        _botConfig = botConfig.Value;
        _telegramBotClient = telegramBotСlient;
    }

    public async Task StartAsync(CancellationToken token = default)
    {
        var absolutePath = $"{_botConfig.Host}/{_botConfig.UpdateRoute}";
        await _telegramBotClient.SetWebhook(
            url: absolutePath,
            allowedUpdates: Array.Empty<UpdateType>(),
            cancellationToken: token);
    }

    public async Task StopAsync(CancellationToken token = default)
    {
        await _telegramBotClient.DeleteWebhook(cancellationToken: token);
    }
}
