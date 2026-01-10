using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;

namespace TelegramBotLogger;

public class TelegramBotLogger : ILogger, IDisposable
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ChatId[] _usersToSend;
    private readonly string _loggerName;
    private static readonly TimeSpan _sendTimeout = TimeSpan.FromSeconds(1);

    private readonly CancellationTokenSource _cts = new();
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public TelegramBotLogger(ITelegramBotClient telegramBotClient, ChatId[] usersToSend, string loggerName)
    {
        ArgumentNullException.ThrowIfNull(telegramBotClient, nameof(telegramBotClient));
        _telegramBotClient = telegramBotClient;

        ArgumentNullException.ThrowIfNull(usersToSend, nameof(usersToSend));
        _usersToSend = usersToSend;

        ArgumentException.ThrowIfNullOrWhiteSpace(loggerName, nameof(loggerName));
        _loggerName = loggerName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => logLevel is LogLevel.Warning or LogLevel.Error or LogLevel.Critical;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel) || formatter == null)
            return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
            return;

        var logParameters = state as IReadOnlyList<KeyValuePair<string, object?>>;

        Task.Run(() => SendToTelegramBotAsync(message, logLevel, exception, logParameters, _usersToSend, _cts.Token));
    }

    private async Task SendToTelegramBotAsync(
        string message, 
        LogLevel logLevel, 
        Exception? excpetion,
        IReadOnlyList<KeyValuePair<string, object?>>? logParameters,
        ChatId[] usersToSend,
        CancellationToken token = default)
    {
        await _semaphore.WaitAsync(token);

        try
        {
            var sb = new StringBuilder();

            sb.AppendLine($"<b>{logLevel}</b> <code>{DateTimeOffset.Now}</code>");

            sb.AppendLine($"<code>{EscapeTelegramChars(_loggerName)}</code>");

            sb.AppendLine(string.Empty);

            sb.AppendLine($"<b>Message:</b> {EscapeTelegramChars(message)}");

            sb.AppendLine(string.Empty);

            if (excpetion != null)
            {
                sb.AppendLine($"<b>Exception:</b> {excpetion.GetType()}");

                if (excpetion.StackTrace != null)
                {
                    var stackTrace = EscapeTelegramChars(excpetion.StackTrace);

                    sb.AppendLine($"<pre>{stackTrace}</pre>");
                }
            }

            if (logParameters != null)
            {
                sb.AppendLine("Параметры:");
                sb.AppendLine("<blockquote>");

                foreach (var (key, value) in logParameters)
                {
                    sb.AppendLine($"{EscapeTelegramChars(key)}: {EscapeTelegramChars(value?.ToString())}");
                }

                sb.AppendLine("</blockquote>");
            }

            var logMessage = sb.ToString();

            // Тут максимум 30 пользователей можно
            foreach (var userId in usersToSend)
            {
                await _telegramBotClient.SendHtml(userId, logMessage);
            }

            // Чтобы не превысить ограничения телеги
            await Task.Delay(_sendTimeout, token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to log to bot logger: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _semaphore?.Dispose();
    }

    private static string EscapeTelegramChars(string? text) 
        => text is null ? "null" : text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
