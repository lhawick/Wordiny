namespace Wordiny.DataAccess.Models;

public class User
{
    public long Id { get; protected set; }
    public long ChatId { get; protected set; }
    public string Username { get; protected set; } = string.Empty;
    public bool IsDisabled { get; protected set; } = false;
    public DateTimeOffset Created { get; protected set; }

    public List<Phrase> Phrases { get; protected set; } = [];
    public UserSettings? Settings { get; protected set; }

    public User(long userId, long chatId, string username)
    {
        Id = userId;
        ChatId = chatId;
        Username = username;
        Created = DateTimeOffset.UtcNow;
    }

    public User Disable()
    {
        IsDisabled = true;

        return this;
    }

    public User Enable()
    {
        IsDisabled = false;

        return this;
    }
}
