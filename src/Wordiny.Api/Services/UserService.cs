using Microsoft.EntityFrameworkCore;
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
}

public class UserService : IUserService
{
    private readonly WordinyDbContext _db;

    public UserService(WordinyDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetUserAsync(long userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserExistAsync(long userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
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
        await _db.Users
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(
                x => x.SetProperty(u => u.IsDisabled, false), 
                token);
    }

    public async Task DisabledUserAsync(long userId, CancellationToken token = default)
    {
        await _db.Users
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(
                x => x.SetProperty(u => u.IsDisabled, true),
                token);
    }
}
