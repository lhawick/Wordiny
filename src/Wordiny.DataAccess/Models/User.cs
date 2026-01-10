namespace Wordiny.DataAccess.Models;

public class User
{
    public long Id { get; protected set; }
    public bool IsDisabled { get; protected set; } = false;
    public DateTimeOffset Created { get; protected set; }
    public DateTimeOffset Updated { get; protected set; }
    public UserInputState InputState 
    { 
        get;  
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentException($"Unknow value of enum {nameof(UserInputState)}: {value}", nameof(InputState));
            }

            field = value;
        }
    }

    private readonly List<Phrase> _phrases = [];
    public IEnumerable<Phrase> Phrases => _phrases.AsEnumerable();

    public UserSettings? Settings { get; protected set; }

    public User(long userId)
    {
        Id = userId;
        Created = DateTimeOffset.UtcNow;
        Updated = DateTimeOffset.UtcNow;
    }

    public User Disable()
    {
        IsDisabled = true;
        Updated = DateTimeOffset.UtcNow;

        return this;
    }

    public User Enable()
    {
        IsDisabled = false;
        Updated = DateTimeOffset.UtcNow;

        return this;
    }
}

public enum UserInputState : byte
{
    None = 0,

    SetTimeZone = 1,
    ConfirmTimeZone = 2,
    SetFrequence = 3,

    AwaitingWordAdding = 10,
    AwaitingWordTranslation = 11,
}
