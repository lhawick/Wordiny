using Wordiny.Api.Models;
using Wordiny.DataAccess;

namespace Wordiny.Api.Services;

public interface IUpdateHandler
{
    Task HandleAsync(Telegram.Bot.Types.Update update, CancellationToken token = default);
}

public class UpdateHandler : IUpdateHandler
{
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IMessageHandler _messageHandler;
    private readonly WordinyDbContext _db;

    public UpdateHandler(
        ILogger<UpdateHandler> logger,
        IMessageHandler messageHandler,
        WordinyDbContext db)
    {
        _logger = logger;
        _messageHandler = messageHandler;
        _db = db;
    }

    public async Task HandleAsync(Telegram.Bot.Types.Update update, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(update, nameof(update));

        switch (update)
        {
            case { Message: { } msg }:
                {
                    if (msg.From is null)
                    {
                        _logger.LogError("Update message user is null");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(msg.Text))
                    {
                        _logger.LogError("Update message text is null or empty");
                        break;
                    }

                    if (msg.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        _logger.LogWarning(
                            "Message type is {messageType} instead of {expectedType}",
                            msg.Type,
                            Telegram.Bot.Types.Enums.MessageType.Text);

                        break;
                    }

                    var message = new Message(msg.From.Id, msg.Text);

                    await _messageHandler.HandleAsync(message, token);

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
