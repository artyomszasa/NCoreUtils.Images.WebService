using System;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images;

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = FailedProcess,
        EventName = nameof(FailedProcess),
        Level = LogLevel.Information,
        Message = "Failed to process request."
    )]
    public static partial void LogFailedProcess(this ILogger logger, Exception exn);
}