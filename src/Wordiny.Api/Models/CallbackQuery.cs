namespace Wordiny.Api.Models;

public record CallbackQuery(long UserId, int MessageId, string Data);
