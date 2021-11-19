using System;
using System.Runtime.CompilerServices;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool Eqi(string a, string b)
            => StringComparer.OrdinalIgnoreCase.Equals(a, b);

        private readonly RequestDelegate _next;

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

        public Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            // var (s, e) = GetSegment(request.Path.Value);
            // string path = request.Path.Value.Substring(s, e);
            if (request.Path == _resizeEndpoint && Eqi(request.Method, "POST"))
            {
                var resourceFactory = context.RequestServices.GetRequiredService<IResourceFactory>();
                var resizer = context.RequestServices.GetRequiredService<IImageResizer>();
                var fun = ActivatorUtilities.CreateInstance<CoreFunctions>(context.RequestServices);
                return fun.InvokeResize(request, resourceFactory, resizer, context.RequestAborted);
            }
            if (request.Path == _capabilitiesEndpoint && Eqi(request.Method, "GET"))
            {
                var fun = ActivatorUtilities.CreateInstance<CoreFunctions>(context.RequestServices);
                return fun.InvokeCapabilities(context.Response, context.RequestAborted);
            }
            if (request.Path == _infoEndpoint && Eqi(request.Method, "POST"))
            {
                var resourceFactory = context.RequestServices.GetRequiredService<IResourceFactory>();
                var analyzer = context.RequestServices.GetRequiredService<IImageAnalyzer>();
                var fun = ActivatorUtilities.CreateInstance<CoreFunctions>(context.RequestServices);
                return fun.InvokeAnalyze(request, context.Response, resourceFactory, analyzer, context.RequestAborted);
            }
            return _next(context);
        }
    }
}