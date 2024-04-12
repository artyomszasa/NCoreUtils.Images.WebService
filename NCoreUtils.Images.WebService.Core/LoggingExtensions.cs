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

    [LoggerMessage(
        EventId = SuccessfullyPerformedResizeOperation,
        EventName = nameof(SuccessfullyPerformedResizeOperation),
        Level = LogLevel.Information,
        Message = "Successfully performed resize operation [Source = {Source}, Destination = {Destination}, Options = {Options}]."
    )]
    public static partial void LogSuccessfullyPerformedResizeOperation(this ILogger logger, IReadableResource source, IWritableResource destination, ResizeOptions options);

    [LoggerMessage(
        EventId = SuccessfullyPerformedAnalyzeOperation,
        EventName = nameof(SuccessfullyPerformedAnalyzeOperation),
        Level = LogLevel.Information,
        Message = "Successfully performed analyze operation [Source = {Source}]."
    )]
    public static partial void LogSuccessfullyPerformedAnalyzeOperation(this ILogger logger, IReadableResource source);
}