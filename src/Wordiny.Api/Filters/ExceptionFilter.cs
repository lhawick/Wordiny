namespace Wordiny.Api.Filters;

public class ExceptionFilter : IEndpointFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
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
