using System.Data.Common;
using Wordiny.Api.Exceptions;
using Wordiny.Api.Services;
using Wordiny.DataAccess;

namespace Wordiny.Api.Filters;

public class ExceptionFilter : IEndpointFilter
{
    private readonly ILogger<ExceptionFilter> _logger;
    private readonly WordinyDbContext _db;
    private readonly IUserService _userService;

    private static readonly Type[] _exceptionsTypesToRetryUpdate = [typeof(DbException), typeof(TelegramSendMessageException)];

    public ExceptionFilter(ILogger<ExceptionFilter> logger, WordinyDbContext db, IUserService userService)
    {
        _logger = logger;
        _db = db;
        _userService = userService;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured in filter: {errorMessage}", ex.Message);

            return Results.InternalServerError();
        }
    }
}
