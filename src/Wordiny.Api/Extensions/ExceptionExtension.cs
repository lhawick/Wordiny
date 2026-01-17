using System.Text;

namespace Wordiny.Api.Extensions;

public static class ExceptionExtension
{
    public static string GetFullExceptionMessage(this Exception exception)
    {
        var sb = new StringBuilder(exception.Message);
        var ex = exception.InnerException;

        while (ex != null)
        {
            sb.AppendLine($"Inner exception: {ex.Message}");
            ex = ex.InnerException;
        }

        return sb.ToString();
    }
}
