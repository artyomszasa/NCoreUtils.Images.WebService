using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
namespace NCoreUtils.Images;

internal static partial class LoggingExtensions
{
    [LoggerMessage(
        EventId = RemoteServerSupports,
        EventName = nameof(RemoteServerSupports),
        Level = LogLevel.Debug,
        Message = "Remote server supports following extensions: {Capabilities}."
    )]
    public static partial void LogRemoteServerSupports(this ILogger logger, HashSet<string> capabilities);

    [LoggerMessage(
        EventId = QueryingExtensionsFailed,
        EventName = nameof(QueryingExtensionsFailed),
        Level = LogLevel.Warning,
        Message = "Querying extensions from {Endpoint} failed ({Message}), assuming no extensions supported."
    )]
    public static partial void LogQueryingExtensionsFailed(this ILogger logger, string endpoint, string message);

    [LoggerMessage(
        EventId = InlineDataWillBeUsedDueToServerSettings,
        EventName = nameof(InlineDataWillBeUsedDueToServerSettings),
        Level = LogLevel.Warning,
        Message = "Source image supports json serialization but remote server does not thus inline data will be used."
    )]
    public static partial void LogInlineDataWillBeUsedDueToServerSettings(this ILogger logger);

    [LoggerMessage(
        EventId = AnalyzeOperationStarting,
        EventName = nameof(AnalyzeOperationStarting),
        Level = LogLevel.Debug,
        Message = "Analyze operation starting."
    )]
    public static partial void LogAnalyzeOperationStarting(this ILogger logger);

    [LoggerMessage(
        EventId = ComputedContextForAnalyzeOperation,
        EventName = nameof(ComputedContextForAnalyzeOperation),
        Level = LogLevel.Debug,
        Message = "Computed context for analyze operation ({ContentType})."
    )]
    public static partial void LogComputedContextForAnalyzeOperation(this ILogger logger, object? contentType);

    [LoggerMessage(
        EventId = SendingAnalyzeRequest,
        EventName = nameof(SendingAnalyzeRequest),
        Level = LogLevel.Debug,
        Message = "Sending analyze request."
    )]
    public static partial void LogSendingAnalyzeRequest(this ILogger logger);

    [LoggerMessage(
        EventId = ReceivedAnalyzeResponse,
        EventName = nameof(ReceivedAnalyzeResponse),
        Level = LogLevel.Debug,
        Message = "Received response of the analyze request."
    )]
    public static partial void LogReceivedAnalyzeResponse(this ILogger logger);

    [LoggerMessage(
        EventId = AnalyzeResponseProcessed,
        EventName = nameof(AnalyzeResponseProcessed),
        Level = LogLevel.Debug,
        Message = "Done processing response of the analyze request."
    )]
    public static partial void LogAnalyzeResponseProcessed(this ILogger logger);

    [LoggerMessage(
        EventId = InitializingAnalyzeOperation,
        EventName = nameof(InitializingAnalyzeOperation),
        Level = LogLevel.Debug,
        Message = "Initializing analyze operation."
    )]
    public static partial void LogInitializingAnalyzeOperation(this ILogger logger);

    [LoggerMessage(
        EventId = AnalyzeOperationCompleted,
        EventName = nameof(AnalyzeOperationCompleted),
        Level = LogLevel.Debug,
        Message = "Analyze operation completed."
    )]
    public static partial void LogAnalyzeOperationCompleted(this ILogger logger);

    [LoggerMessage(
        EventId = ConnectionErrorRetry,
        EventName = nameof(ConnectionErrorRetry),
        Level = LogLevel.Warning,
        Message = "Failed to perform operation due to connection error, retrying..."
    )]
    public static partial void LogConnectionErrorRetry(this ILogger logger, Exception exn);

    [LoggerMessage(
        EventId = SourceIsNotSerializable,
        EventName = nameof(SourceIsNotSerializable),
        Level = LogLevel.Warning,
        Message = "Json serializable destination is ignored as source is not json serializable."
    )]
    public static partial void LogSourceIsNotSerializable(this ILogger logger);

    [LoggerMessage(
        EventId = ResizeOperationStarting,
        EventName = nameof(ResizeOperationStarting),
        Level = LogLevel.Debug,
        Message = "Resize operation starting."
    )]
    public static partial void LogResizeOperationStarting(this ILogger logger);

    [LoggerMessage(
        EventId = ResizeContextComputed,
        EventName = nameof(ResizeContextComputed),
        Level = LogLevel.Debug,
        Message = "Computed context for resize operation ({ContentType})."
    )]
    public static partial void LogResizeContextComputed(this ILogger logger, object? contentType);

    [LoggerMessage(
        EventId = CreatingResizerConsumer,
        EventName = nameof(CreatingResizerConsumer),
        Level = LogLevel.Debug,
        Message = "Creating consumer for resize operation"
    )]
    public static partial void LogCreatingResizerConsumer(this ILogger logger);

    [LoggerMessage(
        EventId = SendingResizeRequest,
        EventName = nameof(SendingResizeRequest),
        Level = LogLevel.Debug,
        Message = "Sending resize request."
    )]
    public static partial void LogSendingResizeRequest(this ILogger logger);

    [LoggerMessage(
        EventId = ReceivedResizeResponse,
        EventName = nameof(ReceivedResizeResponse),
        Level = LogLevel.Debug,
        Message = "Received response of the resize request."
    )]
    public static partial void LogReceivedResizeResponse(this ILogger logger);

    [LoggerMessage(
        EventId = ResizeResponseProcessed,
        EventName = nameof(ResizeResponseProcessed),
        Level = LogLevel.Debug,
        Message = "Done processing response of the resize request."
    )]
    public static partial void LogResizeResponseProcessed(this ILogger logger);

    [LoggerMessage(
        EventId = InitializingResizeOperation,
        EventName = nameof(InitializingResizeOperation),
        Level = LogLevel.Debug,
        Message = "Initializing resize operation."
    )]
    public static partial void LogInitializingResizeOperation(this ILogger logger);

    [LoggerMessage(
        EventId = ResizeOperationCompleted,
        EventName = nameof(ResizeOperationCompleted),
        Level = LogLevel.Debug,
        Message = "Resize operation completed."
    )]
    public static partial void LogResizeOperationCompleted(this ILogger logger);

}
