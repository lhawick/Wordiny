using System.ComponentModel;

namespace Wordiny.DataAccess.Models;

public class UserSettings
{
    public long UserId { get; protected set; }
    public SettingsStep SettingsSetupStep { get; protected set; }

    private RepeatFrequencyInDay _repeatFrequencyInDay;
    public RepeatFrequencyInDay RepeatFrequencyInDay 
    { 
        get { return _repeatFrequencyInDay; }
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new InvalidEnumArgumentException(nameof(RepeatFrequencyInDay), (int)value, typeof(RepeatFrequencyInDay));
            }
            
            _repeatFrequencyInDay = value;
        } 
    }

    private short? _timezone;
    public short? Timezone 
    { 
        get { return _timezone; }
        set
        {
            if (value is null || value < -12 || value > 14)
            {
                throw new ArgumentException($"Timezone doesn`t exist: {value}", nameof(Timezone));
            }

            _timezone = value;
        }
    }

    public UserSettings(
        long userId, 
        SettingsStep settingsStep = SettingsStep.NoSettings, 
        RepeatFrequencyInDay frequencyInDay = RepeatFrequencyInDay.None)
    {
        UserId = userId;
        SettingsSetupStep = settingsStep;
        RepeatFrequencyInDay = frequencyInDay;
    }

    public void NextSettingsStep()
    {
        if (SettingsSetupStep < SettingsStep.Setupped)
        {
            SettingsSetupStep++;
        }
    }
}

public enum SettingsStep : byte
{
    NoSettings = 0,
    SetTimeZone = 1,
    SetFrequence = 2,
    Setupped = 4
}

public enum RepeatFrequencyInDay : byte
{
    None = 0,
    Three = 3,
    Four = 4,
    Six = 6,
    Tvelwe = 12,
}
