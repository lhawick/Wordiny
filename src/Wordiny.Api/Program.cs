using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Wordiny.Api.Config;

var builder = WebApplication.CreateBuilder(args);

// Logging

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.

builder.Services.Configure<BotConfig>(builder.Configuration.GetSection(nameof(BotConfig)));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>(services =>
{
    var botConfig = services.GetRequiredService<IOptions<BotConfig>>().Value;
    var httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient();
    var botOptions = new TelegramBotClientOptions(botConfig.BotToken, botConfig.Host, builder.Environment.IsDevelopment());

    return new TelegramBotClient(botOptions, httpClient);
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPost("/update", (Update update, ILogger<Program> logger, CancellationToken token = default) =>
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

    return Results.Ok();
});

app.UseHttpsRedirection();
app.Run();