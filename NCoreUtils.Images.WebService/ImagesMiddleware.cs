using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images
{
    public partial class ImagesMiddleware
    {
        readonly RequestDelegate _next;

        private readonly PathString _resizeEndpoint;

        private readonly PathString _capabilitiesEndpoint;

        private readonly PathString _infoEndpoint;

        public ImagesMiddleware(RequestDelegate next)
        {
            var prefix = Environment.GetEnvironmentVariable("ENDPOINT_PREFIX") switch
            {
                null => "/",
                "" => "/",
                var pre => "/" + pre.Trim('/')
            };
            _resizeEndpoint = prefix;
            _capabilitiesEndpoint = prefix + Routes.Capabilities;
            _infoEndpoint = prefix + Routes.Info;
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        async Task InvokeCapabilities(HttpResponse response, CancellationToken cancellationToken)
        {
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength = _capabilities.Length;
            await response.BodyWriter.WriteAsync(_capabilities, cancellationToken).ConfigureAwait(false);
            await response.BodyWriter.CompleteAsync().ConfigureAwait(false);
        }

        async Task InvokeResize(HttpRequest request, IResourceFactory resourceFactory, IImageResizer resizer, CancellationToken cancellationToken)
        {
            var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
            var (source, destination) = ResolveSourceAndDestination(resourceFactory, sourceAndDestination);
            await resizer.ResizeAsync(source, destination, ReadResizeOptions(request.Query), cancellationToken).ConfigureAwait(false);
        }

        async Task InvokeAnalyze(HttpRequest request, HttpResponse response, IResourceFactory resourceFactory, IImageAnalyzer analyzer, CancellationToken cancellationToken)
        {
            var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
            var source = resourceFactory.CreateSource(sourceAndDestination.Source, () => NotSupportedUri<IImageSource>(sourceAndDestination.Source));
            var info = await analyzer.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
            response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(response.Body, info, cancellationToken: cancellationToken).ConfigureAwait(false);
            await response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            // var (s, e) = GetSegment(request.Path.Value);
            // string path = request.Path.Value.Substring(s, e);
            if (request.Path == _resizeEndpoint && Eqi(request.Method, "POST"))
            {
                var resourceFactory = context.RequestServices.GetRequiredService<IResourceFactory>();
                var resizer = context.RequestServices.GetRequiredService<IImageResizer>();
                return InvokeResize(request, resourceFactory, resizer, context.RequestAborted);
            }
            if (request.Path == _capabilitiesEndpoint && Eqi(request.Method, "GET"))
            {
                return InvokeCapabilities(context.Response, context.RequestAborted);
            }
            if (request.Path == _infoEndpoint && Eqi(request.Method, "POST"))
            {
                var resourceFactory = context.RequestServices.GetRequiredService<IResourceFactory>();
                var analyzer = context.RequestServices.GetRequiredService<IImageAnalyzer>();
                return InvokeAnalyze(request, context.Response, resourceFactory, analyzer, context.RequestAborted);
            }
            return _next(context);
        }
    }
}