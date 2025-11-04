using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wordiny.Api.Config;
using Wordiny.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

// Logging

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.

builder.Services.Configure<BotConfig>(builder.Configuration.GetSection(nameof(BotConfig)));

var webHookUrl = builder.Configuration["BotConfig:BotWebHookUrl"] ?? throw new InvalidOperationException("Web hook url is not provided");
var botToken = builder.Configuration["BotConfig:BotToken"] ?? throw new InvalidOperationException("BotToken is not provided");

builder.Services
    .AddHttpClient("tgwebhook")
    .AddTypedClient(httpClient => new TelegramBotClient(botToken, httpClient));

builder.Services.AddHostedService<ConfigureWebhookService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapPost("/update", OnUpdate);

app.Run();

async Task OnUpdate(
    Update update, 
    ILogger<Program> logger,
    TelegramBotClient bot,
    CancellationToken token = default)
{

#if DEBUG
    jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    var updateAsJson = JsonSerializer.Serialize(update, jsonSerializerOptions);
    logger.LogDebug("Received an update event with type {updateType}\n{json}", update.Type, updateAsJson);
#endif

    if (update.Message is null)
    {
        logger.LogWarning("Expected update type is {messageUpdateType}. Got {updateType}", nameof(UpdateType.Message), update.Type);
        return;
    }

    //await bot.SendMessage(update.Message.Chat, $"Сообщение получено: {update.Message.Text}");

    await Task.CompletedTask;
}