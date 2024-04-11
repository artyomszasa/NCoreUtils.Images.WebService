using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images;

internal static partial class LoggingExtensions
{
    public static void LogRemoteServerSupports(this ILogger logger, HashSet<string> capabilities)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(RemoteServerSupports, nameof(RemoteServerSupports)),
            message: "Overwriting localization data for {locale}.",
            args: [string.Join(", ", capabilities)]
        );

    public static void LogQueryingExtensionsFailed(this ILogger logger, string endpoint, string message)
        => logger.Log(
            logLevel: LogLevel.Warning,
            eventId: new EventId(QueryingExtensionsFailed, nameof(QueryingExtensionsFailed)),
            message: "Querying extensions from {Endpoint} failed ({Message}), assuming no extensions supported.",
            args: [endpoint, message]
        );

    public static void LogInlineDataWillBeUsedDueToServerSettings(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Warning,
            eventId: new EventId(InlineDataWillBeUsedDueToServerSettings, nameof(InlineDataWillBeUsedDueToServerSettings)),
            message: "Source image supports json serialization but remote server does not thus inline data will be used."
        );

    public static void LogAnalyzeOperationStarting(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(AnalyzeOperationStarting, nameof(AnalyzeOperationStarting)),
            message: "Analyze operation starting."
        );

    public static void LogComputedContextForAnalyzeOperation(this ILogger logger, object? contentType)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ComputedContextForAnalyzeOperation, nameof(ComputedContextForAnalyzeOperation)),
            message: "Computed context for analyze operation ({ContentType}).",
            args: [contentType]
        );

    public static void LogSendingAnalyzeRequest(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(SendingAnalyzeRequest, nameof(SendingAnalyzeRequest)),
            message: "Sending analyze request."
        );

    public static void LogReceivedAnalyzeResponse(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ReceivedAnalyzeResponse, nameof(ReceivedAnalyzeResponse)),
            message: "Received response of the analyze request."
        );

    public static void LogAnalyzeResponseProcessed(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(AnalyzeResponseProcessed, nameof(AnalyzeResponseProcessed)),
            message: "Done processing response of the analyze request."
        );

    public static void LogInitializingAnalyzeOperation(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(InitializingAnalyzeOperation, nameof(InitializingAnalyzeOperation)),
            message: "Initializing analyze operation."
        );

    public static void LogAnalyzeOperationCompleted(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(AnalyzeOperationCompleted, nameof(AnalyzeOperationCompleted)),
            message: "Analyze operation completed."
        );

    public static void LogConnectionErrorRetry(this ILogger logger, Exception exn)
        => logger.Log(
            logLevel: LogLevel.Warning,
            eventId: new EventId(ConnectionErrorRetry, nameof(ConnectionErrorRetry)),
            message: "Failed to perform operation due to connection error, retrying...",
            exception: exn
        );

    public static void LogSourceIsNotSerializable(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Warning,
            eventId: new EventId(SourceIsNotSerializable, nameof(SourceIsNotSerializable)),
            message: "Json serializable destination is ignored as source is not json serializable."
        );

    public static void LogResizeOperationStarting(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ResizeOperationStarting, nameof(ResizeOperationStarting)),
            message: "Resize operation starting."
        );

    public static void LogResizeContextComputed(this ILogger logger, object? contentType)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ResizeContextComputed, nameof(ResizeContextComputed)),
            message: "Computed context for resize operation ({ContentType}).",
            args: [contentType]
        );

    public static void LogCreatingResizerConsumer(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(CreatingResizerConsumer, nameof(CreatingResizerConsumer)),
            message: "Creating consumer for resize operation."
        );

    public static void LogSendingResizeRequest(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(SendingResizeRequest, nameof(SendingResizeRequest)),
            message: "Sending resize request."
        );

    public static void LogReceivedResizeResponse(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ReceivedResizeResponse, nameof(ReceivedResizeResponse)),
            message: "Received response of the resize request."
        );

    public static void LogResizeResponseProcessed(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ResizeResponseProcessed, nameof(ResizeResponseProcessed)),
            message: "Done processing response of the resize request."
        );

    public static void LogInitializingResizeOperation(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(InitializingResizeOperation, nameof(InitializingResizeOperation)),
            message: "Initializing resize operation."
        );

    public static void LogResizeOperationCompleted(this ILogger logger)
        => logger.Log(
            logLevel: LogLevel.Debug,
            eventId: new EventId(ResizeOperationCompleted, nameof(ResizeOperationCompleted)),
            message: "Resize operation completed."
        );

}

