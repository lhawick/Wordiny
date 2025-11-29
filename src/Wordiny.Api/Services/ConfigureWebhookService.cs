using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Wordiny.Api.Config;

namespace Wordiny.Api.Services;

public class ConfigureWebhookService : IHostedService
{
    private readonly BotConfig _botConfig;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<ConfigureWebhookService> _logger;

    public ConfigureWebhookService(
        IOptions<BotConfig> botConfig,
        ITelegramBotClient telegramBotСlient,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<ConfigureWebhookService> logger)
    {
        _botConfig = botConfig.Value;
        _telegramBotClient = telegramBotСlient;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken token = default)
    {
        _hostApplicationLifetime.ApplicationStarted.Register(async () =>
        {
            await _telegramBotClient.SetWebhook(
                url: _botConfig.BotWebHookUrl,
                allowedUpdates: [UpdateType.Message],
                cancellationToken: token);

            _logger.LogInformation($"Web hook setted");
        });
    }

    public async Task StopAsync(CancellationToken token = default)
    {
        await _telegramBotClient.DeleteWebhook(cancellationToken: token);

        _logger.LogInformation($"Web hook deleted");
    }
}
