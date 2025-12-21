using System;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images;

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = FailedToProcessRequest,
        EventName = nameof(FailedToProcessRequest),
        Level = LogLevel.Warning,
        Message = "Failed to process request."
    )]
    public static partial void LogFailedToProcessRequest(this ILogger logger, Exception exn);
}