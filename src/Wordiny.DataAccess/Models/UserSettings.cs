namespace Wordiny.DataAccess.Models;

public class UserSettings
{
    public long UserId { get; protected set; }
    public SettingsSetupStep SettingsSetupStep { get; protected set; }
    public RepeatFrequencyInDay RepeatFrequencyInDay { get; protected set; }

    public UserSettings(
        long userId, 
        SettingsSetupStep settingsStep = SettingsSetupStep.NoSetting, 
        RepeatFrequencyInDay frequencyInDay = RepeatFrequencyInDay.None)
    {
        UserId = userId;
        SettingsSetupStep = settingsStep;
        RepeatFrequencyInDay = frequencyInDay;
    }
}

public enum SettingsSetupStep : byte
{
    NoSetting = 0,
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
