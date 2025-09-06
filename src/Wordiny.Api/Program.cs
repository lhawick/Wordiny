using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wordiny.Api.Config;

var builder = WebApplication.CreateBuilder(args);

// Logging

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.

builder.Services.Configure<BotConfig>(builder.Configuration.GetSection(nameof(BotConfig)));

var webHookUrl = builder.Configuration["BotConfig:BotWebHookUrl"] ?? throw new Exception("Web hook url is not provided");
var botToken = builder.Configuration["BotConfig:BotToken"] ?? throw new Exception("BotToken is not provided");

builder.Services
    .AddHttpClient("tgwebhook")
    .AddTypedClient(httpClient => new TelegramBotClient(botToken, httpClient));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPost("/update", OnUpdate);

app.UseHttpsRedirection();
app.Run();

async Task OnUpdate(
    Update update, 
    ILogger<Program> logger,
    TelegramBotClient bot,
    CancellationToken token = default)
{

#if DEBUG
    var jsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    var updateAsJson = JsonSerializer.Serialize(update, jsonSerializerOptions);
    logger.LogDebug($"Received an update event with type {update.Type}\n{updateAsJson}");
#endif

    if (update.Message is null)
    {
        logger.LogWarning($"Expected update type is {nameof(UpdateType.Message)}. Got {update.Type}");
        return;
    }

    //await bot.SendMessage(update.Message.Chat, $"Сообщение получено: {update.Message.Text}");
}