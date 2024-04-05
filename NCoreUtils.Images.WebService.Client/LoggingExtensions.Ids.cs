using System.Net.Sockets;

namespace NCoreUtils.Images;

internal static partial class LoggingExtensions
{
    public const int RemoteServerSupports = 11400;

    public const int QueryingExtensionsFailed = 11401;

    public const int ImageJsonSerializationNotSupported = 11402; //2

    public const int AnalyzeOperationStarting = 11403;

    public const int ComputedContextForAnalyzeOperation = 11404;

    public const int SendingAnalyzeRequest = 11405;

    public const int ReceivedAnalyzeResponse = 11406;

    public const int AnalyzeResponseProcessed = 11407;

    public const int AnalyzeOperationInit = 11408;

    public const int AnalyzeOperationCompleted = 11409;

    public const int ConnectionErrorRetry = 11410;

    public const int DestinationSerializationWarning = 11411;

    public const int ResizeOperationStarting = 11412;

    public const int ResizeContextComputed = 11413;

    public const int CreatingConsumer = 11414;

    public const int SendingResizeRequest = 11415;

    public const int ReceivedResizeResponse = 11416;

    public const int ResizeResponseProcessed = 11417;

    public const int ResizeOperationInit = 11418;

    public const int ResizeOperationCompleted = 11419;
}