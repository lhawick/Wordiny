using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
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
        await _telegramBotClient.SetWebhook(
            url: _botConfig.BotWebHookUrl,
            allowedUpdates: [UpdateType.Message],
            cancellationToken: token);
    }

    public async Task StopAsync(CancellationToken token = default)
    {
        await _telegramBotClient.DeleteWebhook(cancellationToken: token);
    }
}
