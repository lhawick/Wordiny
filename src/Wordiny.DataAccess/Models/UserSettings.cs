using System.ComponentModel;

namespace Wordiny.DataAccess.Models;

public class UserSettings
{
    public long UserId
    {
        get;
        protected set
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{nameof(UserId)} cannot less than or equal to 0");
            }

            field = value;
            
            OnUpdate();
        }
    }
    public DateTimeOffset Updated { get; protected set; }
    public User? User { get; protected set; }

    public RepeatFrequencyInDay RepeatFrequencyInDay
    {
        get;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new InvalidEnumArgumentException(nameof(RepeatFrequencyInDay), (int)value, typeof(RepeatFrequencyInDay));
            }
            
            field = value;
            OnUpdate();
        } 
    }

    public string? TimeZone
    {
        get;
        set
        {
            field = value;
            OnUpdate();
        }
    }

    public UserSettings(
        long userId, 
        RepeatFrequencyInDay frequencyInDay = RepeatFrequencyInDay.None)
    {
        UserId = userId;
        RepeatFrequencyInDay = frequencyInDay;
        OnUpdate();
    }

    protected UserSettings() { }
    
    private void OnUpdate() {
        Updated = DateTimeOffset.UtcNow;
    }
}

public enum RepeatFrequencyInDay : byte
{
    None = 0,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5
}
