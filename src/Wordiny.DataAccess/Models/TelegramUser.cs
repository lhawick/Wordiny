namespace Wordiny.DataAccess.Models;

public class TelegramUser
{
    public long Id { get; protected set; }
    public long ChatId { get; protected set; }
    public string Username { get; protected set; } = string.Empty;
    public bool IsDisabled { get; protected set; } = false;
    public DateTimeOffset Created { get; protected set; }

    public TelegramUser(long userId, long chatId, string username)
    {
        Id = userId;
        ChatId = chatId;
        Username = username;
        Created = DateTimeOffset.UtcNow;
    }

    public TelegramUser Disable()
    {
        IsDisabled = true;

        return this;
    }

    public TelegramUser Enable()
    {
        IsDisabled = false;

        return this;
    }
}
