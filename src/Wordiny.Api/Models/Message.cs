namespace Wordiny.Api.Models;

public class Message
{
    public long UserId { get; }
    public string Text { get; } = string.Empty;
    public Location? Location { get; set; }

    public Message(long userId, string text)
    {
        UserId = userId;
        Text = text;
    }
}

public record Location(double Longitude, double Latitude);
