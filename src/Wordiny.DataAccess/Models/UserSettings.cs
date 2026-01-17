using System.ComponentModel;

namespace Wordiny.DataAccess.Models;

public class UserSettings
{
    public long UserId { get; protected set; }
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
        } 
    }

    public string? TimeZone { get; set; }

    public UserSettings(
        long userId, 
        RepeatFrequencyInDay frequencyInDay = RepeatFrequencyInDay.None)
    {
        UserId = userId;
        RepeatFrequencyInDay = frequencyInDay;
    }

    protected UserSettings() { }
}

public enum RepeatFrequencyInDay : byte
{
    None = 0,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5
}
