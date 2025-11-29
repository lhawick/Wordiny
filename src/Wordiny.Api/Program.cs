using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Wordiny.Api.Config;
using Wordiny.Api.Exceptions;
using Wordiny.Api.Filters;
using Wordiny.Api.Services;
using Wordiny.DataAccess;

var builder = WebApplication.CreateBuilder(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

var exceptionsTypesToRetryUpdate = new Type[] { typeof(DbException), typeof(TelegramSendMessageException) };

// Logging

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.

builder.Services.Configure<BotConfig>(builder.Configuration.GetSection(nameof(BotConfig)));

var botToken = builder.Configuration["BotConfig:BotToken"] ?? throw new InvalidOperationException("BotToken is not provided");

builder.Services
    .AddHttpClient("tgwebhook")
    .AddTypedClient(httpClient => new TelegramBotClient(botToken, httpClient));

builder.Services.AddHostedService<ConfigureWebhookService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

// handlers
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddScoped<IMessageHandler, MessageHandler>();

// services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<ITelegramApiService, TelegramApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapPost("/update", OnUpdate).AddEndpointFilter<ExceptionFilter>();

app.Run();

async Task<IResult> OnUpdate(
    Update update,
    ILogger<Program> logger,
    IUpdateHandler updateHandler,
    WordinyDbContext db,
    IUserService userService,
    ICacheService cacheService,
    CancellationToken token = default)
{

#if DEBUG
    jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    var updateAsJson = JsonSerializer.Serialize(update, jsonSerializerOptions);
    logger.LogDebug("Received an update event with type {updateType}\n{json}", update.Type, updateAsJson);
#endif

    if (update is null)
    {
        logger.LogError("Received empty update message");

        return Results.Ok();
    }

    var transaction = await db.Database.BeginTransactionAsync(token);

    try
    {
        await updateHandler.HandleAsync(update, token);
        await transaction.CommitAsync(token);

        cacheService.Flush();
    }
    catch (UserUndeliverableException ex)
    {
        logger.LogError(ex, "User {userId} undeliverable: {errorMessage}", ex.UserId, ex.Message);

        await transaction.RollbackAsync(token);
        db.ChangeTracker.Clear();

        if (ex.IsDeleted)
        {
            await userService.DeleteUserAsync(ex.UserId, CancellationToken.None);
        }
        else
        {
            await userService.DisabledUserAsync(ex.UserId, CancellationToken.None);
        }

        await db.SaveChangesAsync(CancellationToken.None);

        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Exception occured: {errorMessage}", ex.Message);
        await transaction.RollbackAsync(token);

        var exceptionType = ex.GetType();
        if (exceptionsTypesToRetryUpdate.Contains(exceptionType))
        {
            return Results.InternalServerError();
        }

        return Results.Ok();
    }

    return Results.Ok();
}