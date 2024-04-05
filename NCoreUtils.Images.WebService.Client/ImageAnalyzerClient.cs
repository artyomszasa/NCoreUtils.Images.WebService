using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Images.WebService;
using NCoreUtils.IO;

namespace NCoreUtils.Images;

public partial class ImageAnalyzerClient(
    ImagesClientConfiguration<ImageAnalyzerClient> configuration,
    ILogger<ImagesClient> logger,
    IHttpClientFactory? httpClientFactory = null)
    : ImagesClient(configuration, logger, httpClientFactory)
    , IImageAnalyzer
{
    protected virtual async ValueTask<AnalyzeOperationContext> GetOperationContextAsync(
        IReadableResource source,
        string endpoint,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (source is ISerializableResource ssource)
        {
            // source image is json-serializable
            if (await IsJsonSerializationSupportedAsync(endpoint, cancellationToken).ConfigureAwait(false))
            {
                // both remote server and image source support json-serialization
                // NOTE: Destination is always inline --> not used on server
                var payload = new SourceAndDestination(await ssource.GetUriAsync(cancellationToken), null);
                var producer = StreamProducer.Create((ouput, cancellationToken) =>
                {
                    return new ValueTask(JsonSerializer.SerializeAsync(
                        ouput,
                        payload,
                        SourceAndDestinationJsonContext.Default.SourceAndDestination,
                        cancellationToken)
                    );
                });
                return AnalyzeOperationContext.Json(producer);
            }
            // remote server does not support json-serialization
            if (Configuration.AllowInlineData)
            {
                // remote server does not support json-serialized images but the inline data is enabled --> proceed
                Logger.LogImageJsonSerializationNotSupported();
                return AnalyzeOperationContext.Inline(source.CreateProducer());
            }
            // remote server does not support json-serialized images and the inline data is disabled --> throw exception
            throw new InvalidOperationException("Source image supports json serialization but remote server does not. Either enable inline data or use compatible server.");
        }
        // source image is not json-serializable
        if (!Configuration.AllowInlineData)
        {
            throw new InvalidOperationException("Source image does not support json serialization. Either enable inline data or use json-serializable image source.");
        }
        return AnalyzeOperationContext.Inline(source.CreateProducer());
    }

    protected virtual async ValueTask<ImageInfo> InvokeGetImageInfoAsync(
        IReadableResource source,
        string endpoint,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = Logger.BeginScope(Guid.NewGuid());
        Logger.LogAnalyzeOperationStarting();
        var uri = new UriBuilder(endpoint).AppendPathSegment(Routes.Info).Uri;
        var context = await GetOperationContextAsync(source, endpoint, cancellationToken);
        Logger.LogComputedContextForAnalyzeOperation(context.ContentType);
        try
        {
            var consumer = StreamConsumer
                .Create((input, cancellationToken) => {
                    return JsonSerializer.DeserializeAsync(input, ImageInfoJsonContext.Default.ImageInfo, cancellationToken);
                })
                .Chain(StreamTransformation.Create(async (input, output, cancellationToken) =>
                {
                    Logger.LogSendingAnalyzeRequest();
                    using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new TypedStreamContent(input, context.ContentType) };
                    using var client = CreateHttpClient();
                    using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    Logger.LogReceivedAnalyzeResponse();
                    await CheckAsync(response, cancellationToken);
                    await using var stream =
#if NETSTANDARD2_1
                        await response.Content.ReadAsStreamAsync()
#else
                        await response.Content.ReadAsStreamAsync(cancellationToken)
#endif
                        .ConfigureAwait(false);
                    await stream.CopyToAsync(output, 16 * 1024, cancellationToken)
                        .ConfigureAwait(false);
                    Logger.LogAnalyzeResponseProcessed();
                }));
            Logger.LogAnalyzeOperationInit();
            var result = await context.Producer.ConsumeAsync(consumer, cancellationToken).ConfigureAwait(false);
            Logger.LogAnalyzeOperationCompleted();
            return result ?? new ImageInfo(default, default, default, default, new Dictionary<string, string>(), new Dictionary<string, string>());
        }
        catch (Exception exn) when (exn is not ImageException)
        {
            if (IsSocketRelated(exn, out var socketExn))
            {
                if (IsBrokenPipe(socketExn) && source.Reusable)
                {
                    Logger.LogConnectionErrorRetry(exn);
                    return await InvokeGetImageInfoAsync(source, endpoint, cancellationToken);
                }
                throw new RemoteImageConnectivityException(endpoint, socketExn.SocketErrorCode, "Network related error has occured while performing operation.", exn);
            }
            throw new RemoteImageException(endpoint, ErrorCodes.GenericError, "Error has occurred while performing operation.", exn);
        }
    }

    public virtual ValueTask<ImageInfo> AnalyzeAsync(IReadableResource source, CancellationToken cancellationToken = default)
        => InvokeGetImageInfoAsync(source, Configuration.EndPoint, cancellationToken);
}