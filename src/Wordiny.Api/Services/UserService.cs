using Microsoft.EntityFrameworkCore;
using Wordiny.Api.Exceptions;
using Wordiny.DataAccess;
using Wordiny.DataAccess.Models;

namespace Wordiny.Api.Services;

public interface IUserService
{
    Task<User?> GetUserAsync(long userId, CancellationToken token = default);
    Task<bool> IsUserExistAsync(long userId, CancellationToken token = default);
    Task<User?> AddUserAsync(long userId, CancellationToken token = default);
    Task DeleteUserAsync(long userId, CancellationToken token = default);
    Task EnableUserAsync(long userId, CancellationToken token = default);
    Task DisabledUserAsync(long userId, CancellationToken token = default);
    Task<UserInputState> GetInputStateAsync(long userId, CancellationToken token = default);
    Task SetInputStateAsync(long userId, UserInputState state, CancellationToken token = default);
    Task SetTimeZoneAsync(long userId, string timeZone, CancellationToken token = default);
    Task SetRepeatFrequencyInDayAsync(long userId, RepeatFrequencyInDay frequency, CancellationToken token = default);
}

public class UserService : IUserService
{
    private readonly WordinyDbContext _db;
    private readonly ILogger<UserService> _logger;

    public UserService(WordinyDbContext db, ILogger<UserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(long userId, CancellationToken token = default)
    {
        return await _db.Users.FindAsync([userId], cancellationToken: token);
    }

    public async Task<bool> IsUserExistAsync(long userId, CancellationToken token = default)
    {
        return await _db.Users.AnyAsync(x => x.Id == userId, token);
    }

    public async Task<User?> AddUserAsync(long userId, CancellationToken token = default)
    {
        var isUserExists = await IsUserExistAsync(userId, token);
        if (isUserExists)
        {
            return null;
        }

        var newUser = new User(userId);
        _db.Users.Add(newUser);

        await _db.SaveChangesAsync(token);

        return newUser;
    }

    public async Task DeleteUserAsync(long userId, CancellationToken token = default)
    {
        await _db.Users.Where(x => x.Id == userId).ExecuteDeleteAsync(token);
    }

    public async Task EnableUserAsync(long userId, CancellationToken token = default)
    {
        var user = await _db.Users.FindAsync([userId], cancellationToken: token);
        if (user is null)
        {
            _logger.LogWarning("Cannot enable the user {userId}: user doesn't exist", userId);
            return;
        }

        user.Enable();

        await _db.SaveChangesAsync(token);
    }

    public async Task DisabledUserAsync(long userId, CancellationToken token = default)
    {
        var user = await _db.Users.FindAsync([userId], cancellationToken: token);
        if (user is null)
        {
            _logger.LogWarning("Cannot enable the user {userId}: user doesn't exist", userId);
            return;
        }

        user.Disable();

        await _db.SaveChangesAsync(token);
    }

    public async Task<UserInputState> GetInputStateAsync(long userId, CancellationToken token = default)
    {
        var user = await GetUserAsync(userId, token) ?? throw new UserNotFoundException(userId);

        return user.InputState;
    }

    public async Task SetInputStateAsync(long userId, UserInputState state, CancellationToken token = default)
    {
        var user = await GetUserAsync(userId, token) ?? throw new UserNotFoundException(userId);

        user.InputState = state;

        await _db.SaveChangesAsync(token);
    }

    public async Task SetTimeZoneAsync(long userId, string timeZone, CancellationToken token = default)
    {
        var userSettings = await _db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId, token);
        if (userSettings is null)
        {
            userSettings = new(userId);
            _db.Add(userSettings);
        }

        userSettings.TimeZone = timeZone;

        await _db.SaveChangesAsync(token);
    }

    public async Task SetRepeatFrequencyInDayAsync(long userId, RepeatFrequencyInDay frequency, CancellationToken token = default)
    {
        var userSettings = await _db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId, token);
        if (userSettings is null)
        {
            userSettings = new(userId);
            _db.Add(userSettings);
        }

        userSettings.RepeatFrequencyInDay = frequency;

        await _db.SaveChangesAsync(token);
    }
}
