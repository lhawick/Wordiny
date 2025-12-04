namespace Wordiny.Api.Exceptions;

public class UserNotFoundException : Exception
{
    public long UserId { get; }

    public UserNotFoundException(long userId) : base($"The user {userId} not found")
    {
        UserId = userId;
    }
}
