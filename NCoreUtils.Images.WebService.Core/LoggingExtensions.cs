using System;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images.WebService;

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = DirectoryNotExist,
        EventName = nameof(DirectoryNotExist),
        Level = LogLevel.Warning,
        Message = "Specified directory does not exist: {Path}"
    )]
    public static partial void LogDirectoryNotExist(this ILogger logger, string path);

    [LoggerMessage(
        EventId = CleanupSuccess,
        EventName = nameof(CleanupSuccess),
        Level = LogLevel.Information,
        Message = "Succesfully removed {Path} as part of cleanup."
    )]
    public static partial void LogCleanupSuccess(this ILogger logger, string path);

    [LoggerMessage(
        EventId = FailedToRemove,
        EventName = nameof(FailedToRemove),
        Level = LogLevel.Warning,
        Message = "Failed to remove {Path} as part of cleanup."
    )]
    public static partial void LogFailedToRemove(this ILogger logger, Exception exn, string path);

    [LoggerMessage(
        EventId = FailedToProcessRequest,
        EventName = nameof(FailedToProcessRequest),
        Level = LogLevel.Warning,
        Message = "Failed to process request."
    )]
    public static partial void LogFailedToProcessRequest(this ILogger logger, Exception exn);
}