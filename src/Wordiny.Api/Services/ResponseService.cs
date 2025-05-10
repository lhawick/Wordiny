using Telegram.Bot.Types;
using Wordiny.Api.Models;

namespace Wordiny.Api.Services;

public interface IResponseService
{
    Task<ResponseMessages> GetResponseAsync(Update update, CancellationToken token = default);
}

public class ResponseService
{
}
