using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCoreUtils.Images.Internal;

namespace NCoreUtils.Images;

public static class CoreFunctions
{
    public static Task InvokeCapabilities(HttpResponse response, CancellationToken cancellationToken)
        => Generic.CoreFunctions.InvokeCapabilities(
            new AspNetCoreHttpResponseWrapper(response),
            cancellationToken
        );

    public static Task InvokeResize(
        HttpRequest request,
        IResourceFactory resourceFactory,
        IImageResizer resizer,
        ILogger logger,
        CancellationToken cancellationToken)
        => Generic.CoreFunctions.InvokeResize(
            new AspNetCoreHttpRequestWrapper(request),
            resourceFactory,
            resizer,
            logger,
            cancellationToken
        );

    public static ValueTask<ImageInfo> InvokeAnalyze(
        HttpRequest request,
        IResourceFactory resourceFactory,
        IImageAnalyzer analyzer,
        ILogger logger,
        CancellationToken cancellationToken)
        => Generic.CoreFunctions.InvokeAnalyze(
            new AspNetCoreHttpRequestWrapper(request),
            resourceFactory,
            analyzer,
            logger,
            cancellationToken
        );

    public static Task InvokeAnalyze(
        HttpRequest request,
        HttpResponse response,
        IResourceFactory resourceFactory,
        IImageAnalyzer analyzer,
        ILogger logger,
        CancellationToken cancellationToken)
        => Generic.CoreFunctions.InvokeAnalyze(
            new AspNetCoreHttpRequestWrapper(request),
            new AspNetCoreHttpResponseWrapper(response),
            resourceFactory,
            analyzer,
            logger,
            cancellationToken
        );
}