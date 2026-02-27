namespace MMKiwi.PodderNet.MinimalApi;

internal static partial class LogMessages
{
    [LoggerMessage(0, LogLevel.Trace, "Failed login attend {username}")]
    public static partial void FailedAttempt(this ILogger<Auth> logger, string username);
}