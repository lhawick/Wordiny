using Wordiny.DataAccess;
using Wordiny.DataAccess.Models;

namespace Wordiny.Api.Services;

public interface IUserService
{
    Task<User?> GetUserAsync(long userId, CancellationToken token = default);
    Task<bool> IsUserExistAsync(long userId, CancellationToken token = default);
    Task AddUserAsync(long userId, CancellationToken token = default);
    Task DeleteUserAsync(long userId, CancellationToken token = default);
    Task EnableUserAsync(long userId, CancellationToken token = default);
    Task DisabledUserAsync(long userId, CancellationToken token = default);
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

    public Task<User?> GetUserAsync(long userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserExistAsync(long userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task AddUserAsync(long userId, CancellationToken token = default)
    {
        var isUserExists = await IsUserExistAsync(userId, token);
        if (isUserExists)
        {
            _logger.LogError("User {userId} is already exist", userId);
            return;
        }

        var newUser = new User(userId);
    }

    public Task DeleteUserAsync(long userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task EnableUserAsync(long userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task DisabledUserAsync(long userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
