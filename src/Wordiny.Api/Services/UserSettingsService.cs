using Microsoft.EntityFrameworkCore;
using Wordiny.DataAccess;
using Wordiny.DataAccess.Models;

namespace Wordiny.Api.Services;

public interface IUserSettingsService
{
    Task StartSettingAsync(long userId, CancellationToken token = default);
    Task<SettingsStep> GetSettingStepAsync(long userId, CancellationToken token = default);
    Task SetupTimeZoneAsync(long userId, short timeZone, CancellationToken token = default);
    Task SetupRepeatFrequencyInDayAsync(long userId, RepeatFrequencyInDay frequency, CancellationToken token = default);
}

public class UserSettingsService : IUserSettingsService
{
    private readonly WordinyDbContext _db;
    private readonly ICacheService _cache;

    private static readonly TimeSpan _userSettingCacheExpiration = TimeSpan.FromHours(1);

    public UserSettingsService(
        WordinyDbContext db,
        ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task StartSettingAsync(long userId, CancellationToken token = default)
    {
        await _db.UserSettings.Where(x => x.UserId == userId).ExecuteDeleteAsync(token);

        var userSettings = new UserSettings(userId, SettingsStep.SetTimeZone);

        _db.UserSettings.Add(userSettings);
        await _db.SaveChangesAsync(token);

        var cacheKey = GetUserSettingCacheKey(userId);
        _cache.Set(cacheKey, userSettings.SettingsSetupStep, _userSettingCacheExpiration);
    }

    public async Task<SettingsStep> GetSettingStepAsync(long userId, CancellationToken token = default)
    {
        var cacheKey = GetUserSettingCacheKey(userId);
        if (_cache.TryGetValue<SettingsStep>(cacheKey, out var settingStep))
        {
            return settingStep;
        }

        settingStep = await _db.UserSettings.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.SettingsSetupStep)
            .FirstOrDefaultAsync(token);

        _cache.Set(cacheKey, settingStep, _userSettingCacheExpiration);

        return settingStep;
    }

    private static string GetUserSettingCacheKey(long userId) => $"UserSetting:{userId}";

    public async Task SetupTimeZoneAsync(long userId, short timeZone, CancellationToken token = default)
    {
        var settings = await _db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId, token);
        if (settings is null)
        {
            throw new InvalidOperationException($"User {userId} has not started setting up yet");
        }

        if (settings.SettingsSetupStep != SettingsStep.SetTimeZone)
        {
            throw new InvalidOperationException(
                $"Current user settings step is {settings.SettingsSetupStep}. " +
                $"Expected {SettingsStep.SetTimeZone}");
        }

        settings.Timezone = timeZone;
        settings.NextSettingsStep();

        await _db.SaveChangesAsync(token);

        var cacheKey = GetUserSettingCacheKey(userId);
        _cache.Set(cacheKey, settings.SettingsSetupStep, _userSettingCacheExpiration);
    }

    public async Task SetupRepeatFrequencyInDayAsync(long userId, RepeatFrequencyInDay frequency, CancellationToken token = default)
    {
        var settings = await _db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId, token);
        if (settings is null)
        {
            throw new InvalidOperationException($"User {userId} has not started setting up yet");
        }

        if (settings.SettingsSetupStep != SettingsStep.SetFrequence)
        {
            throw new InvalidOperationException(
                $"Current user settings step is {settings.SettingsSetupStep}. " +
                $"Expected {SettingsStep.SetFrequence}");
        }

        settings.RepeatFrequencyInDay = frequency;
        settings.NextSettingsStep();

        var cacheKey = GetUserSettingCacheKey(userId);
        _cache.Set(cacheKey, settings.SettingsSetupStep, _userSettingCacheExpiration);
    }
}
