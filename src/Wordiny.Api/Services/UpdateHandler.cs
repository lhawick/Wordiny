using Wordiny.Api.Models;

namespace Wordiny.Api.Services;

public interface IUpdateHandler
{
    Task HandleAsync(Telegram.Bot.Types.Update update, CancellationToken token = default);
}

public class UpdateHandler : IUpdateHandler
{
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IMessageHandler _messageHandler;

    public UpdateHandler(
        ILogger<UpdateHandler> logger,
        IMessageHandler messageHandler)
    {
        _logger = logger;
        _messageHandler = messageHandler;
    }

    public async Task HandleAsync(Telegram.Bot.Types.Update update, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(update, nameof(update));

        switch (update.Type)
        {
            case Telegram.Bot.Types.Enums.UpdateType.Message:
                {
                    if (update.Message is null)
                    {
                        _logger.LogError("Update message is null");
                        break;
                    }
                    if (update.Message.From is null)
                    {
                        _logger.LogError("Update message user is null");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(update.Message.Text))
                    {
                        _logger.LogError("Update message text is null or empty");
                        break;
                    }

                    var message = new Message(update.Message.From.Id, update.Message.Text);

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
