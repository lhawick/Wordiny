using System.Data.Common;
using Wordiny.Api.Exceptions;
using Wordiny.Api.Models;
using Wordiny.DataAccess;

namespace Wordiny.Api.Services.Handlers;

public enum UpdateHandleResult
{
    Succes = 1,
    RetryNeeded = 2,
    Error = 3,
}

public interface IUpdateHandler
{
    Task<UpdateHandleResult> HandleAsync(Telegram.Bot.Types.Update update, CancellationToken token = default);
}

public class UpdateHandler : IUpdateHandler
{
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IMessageHandler _messageHandler;
    private readonly ICallbackQueryHandler _callbackQueryHandler;
    private readonly WordinyDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly ITelegramApiService _telegramApiService;

    public UpdateHandler(
        ILogger<UpdateHandler> logger,
        IMessageHandler messageHandler,
        ICallbackQueryHandler callbackQueryHandler,
        WordinyDbContext dbContext,
        IUserService userService,
        ITelegramApiService telegramApiService)
    {
        _logger = logger;
        _messageHandler = messageHandler;
        _callbackQueryHandler = callbackQueryHandler;
        _dbContext = dbContext;
        _userService = userService;
        _telegramApiService = telegramApiService;
    }

    public async Task<UpdateHandleResult> HandleAsync(Telegram.Bot.Types.Update update, CancellationToken token = default)
    {
        using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(token);

        try
        {
            await HandleInnerAsync(update, token);
            await dbTransaction.CommitAsync(token);

            return UpdateHandleResult.Succes;
        }
        catch (UserUndeliverableException ex)
        {
            _logger.LogError(ex, "User {userId} undeliverable: {errorMessage}", ex.UserId, ex.Message);

            await dbTransaction.RollbackAsync(token);
            _dbContext.ChangeTracker.Clear();

            if (ex.IsDeleted)
            {
                await _userService.DeleteUserAsync(ex.UserId, CancellationToken.None);
            }
            else
            {
                await _userService.DisabledUserAsync(ex.UserId, CancellationToken.None);
            }

            await _dbContext.SaveChangesAsync(CancellationToken.None);

            return UpdateHandleResult.Succes;
        }
        catch (TelegramSendMessageException ex)
        {
            _logger.LogError("Failed to send message to user {userId}: {errorMessage}", ex.UserId, ex.Message);

            await dbTransaction.RollbackAsync(token);

            return UpdateHandleResult.RetryNeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured: {errorMessage}", ex.Message);

            if (update?.Message?.From != null)
            {
                await _telegramApiService.SendMessageAsync(
                    update.Message.From.Id,
                    "Простите, что-то пошло не так, попробуйте позже",
                    token: token);
            }

            await dbTransaction.RollbackAsync(token);

            if (ex is DbException)
            {
                return UpdateHandleResult.RetryNeeded;
            }

            return UpdateHandleResult.Error;
        }
    }

    private async Task HandleInnerAsync(Telegram.Bot.Types.Update update, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(update, nameof(update));

        switch (update)
        {
            case  { Message: { } msg }:
                {
                    if (msg.From is null)
                    {
                        _logger.LogError("Update message user is null");
                        break;
                    }

                    if (msg.Type is not Telegram.Bot.Types.Enums.MessageType.Text and not Telegram.Bot.Types.Enums.MessageType.Location)
                    {
                        _logger.LogError(
                            "Message type is {messageType} instead of {textType} or {locationType}",
                            msg.Type,
                            Telegram.Bot.Types.Enums.MessageType.Text,
                            Telegram.Bot.Types.Enums.MessageType.Location);

                        break;
                    }

                    var message = new Message(msg.From.Id, msg.Text ?? string.Empty);
                    if (msg.Location != null)
                    {
                        message.Location = new(msg.Location.Longitude, msg.Location.Latitude);
                    }

                    await _messageHandler.HandleAsync(message, token);

                    break;
                }
            case { CallbackQuery: { } callbackQuery }:
                {
                    if (callbackQuery.From is null)
                    {
                        _logger.LogError("Callback query user is null");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(callbackQuery.Data))
                    {
                        _logger.LogError("Callback query data is empty");
                        break;
                    }

                    var callback = new CallbackQuery(callbackQuery.From.Id, callbackQuery.Data);

                    await _callbackQueryHandler.HandleAsync(callback, token);

                    break;
                }
            default:
                {
                    _logger.LogError("No handlers for update type: {updateType}", update.Type);
                    break;
                }
        }
    }
}
