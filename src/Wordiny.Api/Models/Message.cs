namespace Wordiny.Api.Models;

public class Message
{
    public long UserId { get; }
    public string Text { get; } = string.Empty;

    public Message(long userId, string text)
    {
        UserId = userId;
        Text = text;
    }
}
