using System.ComponentModel;

namespace Wordiny.DataAccess.Models;

public class UserSettings
{
    public long UserId { get; protected set; }
    public DateTimeOffset Updated { get; protected set; }

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

    public short? Timezone 
    {
        get;
        set
        {
            if (value is null || value < -12 || value > 14)
            {
                throw new ArgumentException($"Timezone doesn`t exist: {value}", nameof(Timezone));
            }

            field = value;
        }
    }

    public UserSettings(
        long userId, 
        RepeatFrequencyInDay frequencyInDay = RepeatFrequencyInDay.None)
    {
        UserId = userId;
        RepeatFrequencyInDay = frequencyInDay;
    }
}

public enum RepeatFrequencyInDay : byte
{
    None = 0,
    Three = 3,
    Four = 4,
    Six = 6,
    Tvelwe = 12,
}
