namespace Wordiny.Api.Exceptions;

public class UserUndeliverableException : Exception
{
    public long UserId { get; }
    public bool IsDeleted { get; }

    public UserUndeliverableException(
        long userId, 
        bool isDeleted = false,
        string? message = null) : base($"User {userId} is undeliverable: {message ?? "no details"}")
    {
        UserId = userId;
        IsDeleted = isDeleted;
    }
}
