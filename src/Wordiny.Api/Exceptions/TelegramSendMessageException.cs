namespace Wordiny.Api.Exceptions;

public class TelegramSendMessageException : Exception
{
    public long UserId { get; }

    public TelegramSendMessageException(long userId, string? message = null) : base($"Failed to send message to user {userId}: {message ?? "no details"}")
    {
        UserId = userId;
    }
}
