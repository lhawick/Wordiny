using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotLogger;
using Wordiny.Api.Config;
using Wordiny.Api.Exceptions;
using Wordiny.Api.Filters;
using Wordiny.Api.Services;
using Wordiny.Api.Services.Handlers;
using Wordiny.DataAccess;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

var exceptionsTypesToRetryUpdate = new Type[] { typeof(DbException), typeof(TelegramSendMessageException) };

// Logging

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddSingleton<ILoggerProvider, TelegramBotLoggerProvider>(x =>
{
    var telegramBotClient = x.GetRequiredKeyedService<ITelegramBotClient>("WordinyLogger");
    var usersGettingLogs = builder.Configuration.GetSection("WordinyLoggerBotConfig:UsersGettingLogs").Get<long[]>();

    if (usersGettingLogs is null)
    {
        throw new InvalidOperationException("No users to getting logs");
    }

    return new TelegramBotLoggerProvider(usersGettingLogs.Select(x => new ChatId(x)).ToArray(), telegramBotClient);
});

// Add services to the container.

builder.Services.Configure<WordinyBotConfig>(builder.Configuration.GetSection(nameof(WordinyBotConfig)));

builder.Services.AddHttpClient("Wordiny");
builder.Services.AddHttpClient("WordinyLogger").RemoveAllLoggers();

builder.Services.AddKeyedSingleton<ITelegramBotClient, TelegramBotClient>("Wordiny", (services, name) =>
{
    var config = services.GetRequiredService<IConfiguration>();

    var botToken = config["WordinyBotConfig:BotToken"]
        ?? throw new InvalidOperationException("Wordiny BotToken is not provided");

    var httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Wordiny");

    var botOptions = new TelegramBotClientOptions(botToken);

    return new TelegramBotClient(botOptions, httpClient);
});

builder.Services.AddKeyedSingleton<ITelegramBotClient, TelegramBotClient>("WordinyLogger", (services, name) =>
{
    var config = services.GetRequiredService<IConfiguration>();

    var botToken = config["WordinyLoggerBotConfig:BotToken"]
        ?? throw new InvalidOperationException("WordinyLogger BotToken is not provided");

    var httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("WordinyLogger");

    var botOptions = new TelegramBotClientOptions(botToken);

    return new TelegramBotClient(botOptions, httpClient);
});

builder.Services.AddHttpClient<IOxilorApiService, OxilorApiService>((sp, httpClient) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OxilorApi:ApiKey"]
        ?? throw new InvalidOperationException("Oxilor api key is not provided");
    var baseUrl = config["OxilorApi:BaseUrl"] 
        ?? throw new InvalidOperationException("Oxilor api base url is not provided");

    httpClient.BaseAddress = new Uri(baseUrl);
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new("ru"));
});

builder.Services.AddHostedService<ConfigureWebhookService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

// handlers
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddScoped<IMessageHandler, MessageHandler>();
builder.Services.AddScoped<ICallbackQueryHandler, CallbackQueryHandler>();

// services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITelegramApiService, TelegramApiService>();
builder.Services.AddScoped<IPhraseService, PhraseService>();

// database
var wordinyDbConnectionString = builder.Configuration.GetConnectionString("WordinyDb");
if (string.IsNullOrWhiteSpace(wordinyDbConnectionString))
{
    throw new InvalidOperationException("WordinyDb connection string is not provided");
}

builder.Services.AddDbContext<WordinyDbContext>(options =>
{
    options.UseNpgsql(wordinyDbConnectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPost("/update", OnUpdate).AddEndpointFilter<ExceptionFilter>();

app.Run();

async Task<IResult> OnUpdate(
    Update update,
    ILogger<Program> logger,
    IUpdateHandler updateHandler,
    IOptions<WordinyBotConfig> botConfig,
    [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string? secretToken,
    CancellationToken token = default)
{
#if DEBUG
    var updateAsJson = JsonSerializer.Serialize(update, jsonSerializerOptions);
    logger.LogInformation("Received an update event with type {updateType}\n{json}", update.Type, updateAsJson);
#endif
    
    if (string.IsNullOrWhiteSpace(secretToken) || secretToken != botConfig.Value.SecretToken)
    {
        logger.LogError("Invalid secret token: {secretToken}", secretToken);
        return Results.Unauthorized();
    }

    if (update is null)
    {
        logger.LogError("Received empty update message");

        return Results.Ok();
    }

    var handleResult = await updateHandler.HandleAsync(update, token);

    return handleResult switch
    {
        UpdateHandleResult.Success => Results.Ok(),
        UpdateHandleResult.RetryNeeded => Results.InternalServerError(),
        UpdateHandleResult.Error => Results.Ok(),
        _ => Results.Ok(),
    };
}