using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using NCoreUtils.Images.WebService;
using System.Text.Json;
using System.Collections.Generic;
using System.Globalization;

namespace NCoreUtils.Images
{
    public static class ImageFunctions
    {
        private static string[] _capabilities = new [] { Capabilities.JsonSerializedImageInfo };

        static readonly JsonSerializerOptions _sourceAndDestinationSerializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { SourceAndDestinationConverter.Instance }
        };

        private static readonly HashSet<string> _truthy = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "true",
            "t",
            "on",
            "1"
        };

        private static T NotSupportedUri<T>(Uri? uri)
            => throw new ImageException("unsupported_uri", $"Either invalid or unsupported uri: {uri}.");

        private static bool IsJsonCompatible(string contentType)
            => contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
                || contentType.StartsWith("text/json", StringComparison.OrdinalIgnoreCase)
                || contentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase);

        private static ResizeOptions ReadResizeOptions(IQueryCollection query)
        {
            return new ResizeOptions(
                imageType: S("t"),
                width: I("w"),
                height: I("h"),
                resizeMode: S("m"),
                quality: I("q"),
                optimize: B("x"),
                weightX: I("cx"),
                weightY: I("cy"),
                filters: FilterParser.Parse(S("f"))
            );

            bool? B(string name)
            {
                return S(name) switch
                {
                    null => default,
                    string s => _truthy.Contains(s)
                };
            }

            int? I(string name)
            {
                return S(name) switch
                {
                    null => default,
                    string s => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? (int?)i : default
                };
            }

            string? S(string name)
            {
                return query.TryGetValue(name, out var values) && values.Count > 0 ? values[0] : default;
            }
        }

        private static ValueTask<SourceAndDestination> ParseSourceAndDestination(HttpRequest request, CancellationToken cancellationToken)
        {
            if (IsJsonCompatible(request.ContentType))
            {
                return JsonSerializer.DeserializeAsync<SourceAndDestination>(request.Body, _sourceAndDestinationSerializationOptions, cancellationToken);
            }
            return default;
        }

        private static (IImageSource Source, IImageDestination Destination) ResolveSourceAndDestination(IResourceFactory resourceFactory, SourceAndDestination sd)
        {
            var source = resourceFactory.CreateSource(sd.Source, () => NotSupportedUri<IImageSource>(sd.Source));
            var destination = resourceFactory.CreateDestination(sd.Destination, () => NotSupportedUri<IImageDestination>(sd.Destination));
            return (source, destination);
        }

        private static async Task InvokeResize(ILogger logger, HttpRequest request, IResourceFactory resourceFactory, IImageResizer resizer, CancellationToken cancellationToken)
        {
            var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Input: {0}.", sourceAndDestination);
            var (source, destination) = ResolveSourceAndDestination(resourceFactory, sourceAndDestination);
            logger.LogInformation("Successfully resolved source and destination.");
            var options = ReadResizeOptions(request.Query);
            logger.LogInformation("Options: {0}.", options);
            await resizer.ResizeAsync(source, destination, options, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<IActionResult> InvokeAnalyze(HttpRequest request, IResourceFactory resourceFactory, IImageAnalyzer analyzer, CancellationToken cancellationToken)
        {
            var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
            var source = resourceFactory.CreateSource(sourceAndDestination.Source, () => NotSupportedUri<IImageSource>(sourceAndDestination.Source));
            var info = await analyzer.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
            return new JsonSerializedResult<ImageInfo>(info);
        }

        [FunctionName("Resize")]
        public static async Task<IActionResult> RunResize(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "img")] HttpRequest request,
            ILogger log,
            CancellationToken hostCancellationToken)
        {
            // cancellation
            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken, request.HttpContext.RequestAborted);
            // setup DI
            var services = new ServiceCollection();
            new Startup().ConfigureServices(services, log);
            await using var rootServiceProvider = services.BuildServiceProvider(true);
            using (var scope = rootServiceProvider.CreateScope())
            {
                try
                {
                    var resourceFactory = scope.ServiceProvider.GetRequiredService<IResourceFactory>();
                    var resizer = scope.ServiceProvider.GetRequiredService<IImageResizer>();
                    await InvokeResize(log, request, resourceFactory, resizer, cancellationSource.Token);
                }
                catch (Exception exn)
                {
                    log.LogInformation(exn, "Failed to process request.");
                    return new JsonErrorResult(exn);
                }
            }
            return new OkNoCacheResult();
        }

        [FunctionName("Analyze")]
        public static async Task<IActionResult> RunAnalyze(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "img/" + Routes.Info)] HttpRequest request,
            ILogger log,
            CancellationToken hostCancellationToken)
        {
            // cancellation
            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken, request.HttpContext.RequestAborted);
            // setup DI
            var services = new ServiceCollection();
            new Startup().ConfigureServices(services, log);
            await using var rootServiceProvider = services.BuildServiceProvider(true);
            using (var scope = rootServiceProvider.CreateScope())
            {
                try
                {
                    var resourceFactory = scope.ServiceProvider.GetRequiredService<IResourceFactory>();
                    var analyzer = scope.ServiceProvider.GetRequiredService<IImageAnalyzer>();
                    await InvokeAnalyze(request, resourceFactory, analyzer, cancellationSource.Token);
                }
                catch (Exception exn)
                {
                    log.LogInformation(exn, "Failed to process request.");
                    return new JsonErrorResult(exn);
                }
            }
            return new OkResult();
        }

        [FunctionName("Capabilities")]
        public static Task<IActionResult> RunCapabilities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "img/" + Routes.Capabilities)] HttpRequest request,
            ILogger log,
            CancellationToken hostCancellationToken)
        {
            return Task.FromResult<IActionResult>(new JsonSerializedResult<string[]>(_capabilities));
        }
    }
}
