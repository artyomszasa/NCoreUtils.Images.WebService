using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Images.WebService;
using NCoreUtils.IO;

namespace NCoreUtils.Images
{
    public partial class ImageAnalyzerClient : ImagesClient, IImageAnalyzer
    {
        public ImageAnalyzerClient(
            ImagesClientConfiguration<ImageAnalyzerClient> configuration,
            ILogger<ImagesClient> logger,
            IHttpClientFactory? httpClientFactory = null)
            : base(configuration, logger, httpClientFactory)
        { }

        protected virtual async ValueTask<AnalyzeOperationContext> GetOperationContextAsync(
            IImageSource source,
            string endpoint,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (source is ISerializableImageResource ssource)
            {
                // source image is json-serializable
                if (await IsJsonSerializationSupportedAsync(endpoint, cancellationToken).ConfigureAwait(false))
                {
                    // both remote server and image source support json-serialization
                    // NOTE: Destination is always inline --> not used on server
                    var payload = new SourceAndDestination(ssource.Uri, null);
                    var producer = StreamProducer.Create((ouput, cancellationToken) => new ValueTask(JsonSerializer.SerializeAsync(ouput, payload, _sourceAndDestinationSerializationOptions, cancellationToken)));
                    return AnalyzeOperationContext.Json(producer);
                }
                // remote server does not support json-serialization
                if (Configuration.AllowInlineData)
                {
                    // remote server does not support json-serialized images but the inline data is enabled --> proceed
                    Logger.LogWarning("Source image supports json serialization but remote server does not thus inline data will be used.");
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

        protected virtual async Task<ImageInfo> InvokeGetImageInfoAsync(
            IImageSource source,
            string endpoint,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = Logger.BeginScope(Guid.NewGuid());
            Logger.LogDebug("Analyze operation starting.");
            var uri = new UriBuilder(endpoint).AppendPathSegment(Routes.Info).Uri;
            var context = await GetOperationContextAsync(source, endpoint, cancellationToken);
            Logger.LogDebug("Computed context for analyze operation ({0}).", context.ContentType);
            try
            {
                var consumer = StreamConsumer.Create((input, cancellationToken) => JsonSerializer.DeserializeAsync<ImageInfo>(input, cancellationToken: cancellationToken))
                    .Chain(StreamTransformation.Create(async (input, output, cancellationToken) =>
                    {
                        Logger.LogDebug("Sending analyze request.");
                        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new TypedStreamContent(input, context.ContentType) };
                        using var client = CreateHttpClient();
                        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                        Logger.LogDebug("Received response of the analyze request.");
                        await CheckAsync(response, cancellationToken);
                        using var stream = await response.Content.ReadAsStreamAsync();
                        await stream.CopyToAsync(output, 16 * 1024, cancellationToken);
                        Logger.LogDebug("Done processing response of the analyze request.");
                    }));
                Logger.LogDebug("Initializing analyze operation.");
                var result = await context.Producer.ConsumeAsync(consumer, cancellationToken).ConfigureAwait(false);
                Logger.LogDebug("Analyze operation completed.");
                return result;
            }
            catch (Exception exn) when (!(exn is ImageException))
            {
                if (IsSocketRelated(exn, out var socketExn))
                {
                    if (IsBrokenPipe(socketExn) && source.Reusable)
                    {
                        Logger.LogWarning(exn, "Failed to perform operation due to connection error, retrying...");
                        return await InvokeGetImageInfoAsync(source, endpoint, cancellationToken);
                    }
                    throw new RemoteImageConnectivityException(endpoint, socketExn.SocketErrorCode, "Network related error has occured while performing operation.", exn);
                }
                throw new RemoteImageException(endpoint, ErrorCodes.GenericError, "Error has occurred while performing operation.", exn);
            }
        }

        public virtual Task<ImageInfo> AnalyzeAsync(IImageSource source, CancellationToken cancellationToken = default)
            => InvokeGetImageInfoAsync(source, Configuration.EndPoint, cancellationToken);
    }
}