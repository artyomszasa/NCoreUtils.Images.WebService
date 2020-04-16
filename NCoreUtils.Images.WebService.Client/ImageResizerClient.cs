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
    public partial class ImageResizerClient : ImagesClient, IImageResizer
    {
        static string CreateQueryString(ResizeOptions options)
        {
            var first = true;
            Span<char> buffer = stackalloc char[8192];
            var builder = new SpanBuilder(buffer);
            return builder
                .AppendQ(ref first, "t", options.ImageType)
                .AppendQ(ref first, "w", options.Width)
                .AppendQ(ref first, "h", options.Height)
                .AppendQ(ref first, "m", options.ResizeMode)
                .AppendQ(ref first, "q", options.Quality)
                .AppendQ(ref first, "x", options.Optimize)
                .AppendQ(ref first, "cx", options.WeightX)
                .AppendQ(ref first, "cy", options.WeightY)
                .AppendQ(ref first, "f", options.Filters)
                .ToString();
        }

        public ImageResizerClient(
            ImagesClientConfiguration<ImageResizerClient> configuration,
            ILogger<ImageResizerClient> logger,
            IHttpClientFactory? httpClientFactory = null)
            : base(configuration, logger, httpClientFactory)
        { }

        protected virtual async ValueTask<ResizeOperationContext> GetOperationContextAsync(
            IImageSource source,
            IImageDestination destination,
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
                    SourceAndDestination payload;
                    IImageDestination? dest;
                    if (destination is ISerializableImageResource sdestination)
                    {
                        // image destination does also support json-serialization --> destination in payload, no consumer.
                        payload = new SourceAndDestination(ssource.Uri, sdestination.Uri);
                        dest = default;
                    }
                    else
                    {
                        // image destination does also support json-serialization --> null in payload, destination is the consumer.
                        payload = new SourceAndDestination(ssource.Uri, default);
                        dest = destination;
                    }
                    var producer = StreamProducer.Create((ouput, cancellationToken) => new ValueTask(JsonSerializer.SerializeAsync(ouput, payload, _sourceAndDestinationSerializationOptions, cancellationToken)));
                    return ResizeOperationContext.Json(producer, dest);
                }
                // remote server does not support json-serialization
                if (Configuration.AllowInlineData)
                {
                    // remote server does not support json-serialized images but the inline data is enabled --> proceed
                    Logger.LogWarning("Source image supports json serialization but remote server does not thus inline data will be used.");
                    return ResizeOperationContext.Inline(source.CreateProducer(), destination);
                }
                // remote server does not support json-serialized images and the inline data is disabled --> throw exception
                throw new InvalidOperationException("Source image supports json serialization but remote server does not. Either enable inline data or use compatible server.");
            }
            // source image is not json-serializable
            if (!Configuration.AllowInlineData)
            {
                throw new InvalidOperationException("Source image does not support json serialization. Either enable inline data or use json-serializable image source.");
            }
            if (destination is ISerializableImageResource)
            {
                if (Configuration.AllowInlineData)
                {
                    Logger.LogWarning("Json serializable destination is ignored as source is not json serializable.");
                }
                else
                {
                    throw new InvalidOperationException("Destination cannot be json serialized when source is not json serializable.");
                }
            }
            return ResizeOperationContext.Inline(source.CreateProducer(), destination);
        }

        protected virtual async Task InvokeResizeAsync(
            IImageSource source,
            IImageDestination destination,
            string queryString,
            string endpoint,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = Logger.BeginScope(Guid.NewGuid());
            Logger.LogDebug("Resize operation starting.");
            var uri = new UriBuilder(endpoint) { Query = queryString }.Uri;
            var context = await GetOperationContextAsync(source, destination, endpoint, cancellationToken).ConfigureAwait(false);
            Logger.LogDebug("Computed context for resize operation ({0}).", context.ContentType);
            try
            {
                IStreamConsumer consumer;
                if (context.Destination is null)
                {
                    // both source and destination are serializable
                    consumer = StreamConsumer.Create(async (input, cancellationToken) =>
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new JsonStreamContent(input) };
                        using var client = CreateHttpClient();
                        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                        await CheckAsync(response, cancellationToken);
                    });
                }
                else
                {
                    // destination is not serializable, source does either support serialization or not..
                    var outputMime = "application/octet-stream";
                    var finalConsumer = StreamConsumer.Delay(_ =>
                    {
                        Logger.LogDebug("Creating consumer for resize operation");
                        return new ValueTask<IStreamConsumer>(context.Destination.CreateConsumer(new ContentInfo(outputMime)));
                    });
                    consumer = finalConsumer.Chain(StreamTransformation.Create(async (input, output, cancellationToken) =>
                    {
                        Logger.LogDebug("Sending resize request.");
                        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new TypedStreamContent(input, context.ContentType) };
                        using var client = CreateHttpClient();
                        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                        Logger.LogDebug("Received response of the resize request.");
                        await CheckAsync(response, cancellationToken).ConfigureAwait(false);
                        outputMime = response.Content.Headers.ContentType.MediaType ?? "application/octet-stream";
                        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        await stream.CopyToAsync(output, 16 * 1024, cancellationToken).ConfigureAwait(false);
                        Logger.LogDebug("Done processing response of the resize request.");
                    }));
                }
                Logger.LogDebug("Initializing resize operation.");
                await context.Producer.ConsumeAsync(consumer, cancellationToken).ConfigureAwait(false);
                Logger.LogDebug("Resize operation completed.");
            }
            catch (Exception exn) when (!(exn is ImageException))
            {
                if (IsSocketRelated(exn, out var socketExn))
                {
                    if (IsBrokenPipe(socketExn) && source.Reusable)
                    {
                        Logger.LogWarning(exn, "Failed to perform operation due to connection error, retrying...");
                        await InvokeResizeAsync(source, destination, queryString, endpoint, cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    throw new RemoteImageConnectivityException(endpoint, socketExn.SocketErrorCode, "Network related error has occured while performing operation.", exn);
                }
                throw new RemoteImageException(endpoint, ErrorCodes.GenericError, "Error has occurred while performing operation.", exn);
            }
        }

        public virtual Task ResizeAsync(IImageSource source, IImageDestination destination, ResizeOptions options, CancellationToken cancellationToken = default)
        {
            var queryString = CreateQueryString(options);
            return InvokeResizeAsync(source, destination, queryString, Configuration.EndPoint, cancellationToken);
        }
    }
}