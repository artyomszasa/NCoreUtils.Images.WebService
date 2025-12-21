using System.Globalization;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NCoreUtils.Images;

namespace NCoreUtils.AzImages;

internal sealed class HttpRequestAdapter(HttpRequestData request)
    : Images.Generic.IHttpRequest
{
    private HttpRequestData Request { get; } = request ?? throw new ArgumentNullException(nameof(request));

    public string? ContentType
        => Request.Headers.TryGetValues("Content-Type", out var values) ? values.FirstOrDefault() : default;

    public Stream Body => Request.Body;

    public bool TryGetQueryParameter(string key, out StringValues values)
    {
        if (Request.Query.AllKeys.Contains(key))
        {
            var value = Request.Query[key];
            if (!string.IsNullOrEmpty(value))
            {
                values = value;
                return true;
            }
        }
        values = default;
        return false;
    }
}

internal sealed class HttpResponseAdapter(HttpResponseData response)
    : Images.Generic.IHttpResponse
{
    internal HttpResponseData Response { get; } = response ?? throw new ArgumentNullException(nameof(response));

    public string ContentType
    {
        set
        {
            Response.Headers.Remove("Content-Type");
            Response.Headers.Add("Content-Type", value);
        }
    }

    public long? ContentLength
    {
        set
        {
            Response.Headers.Remove("Content-Length");
            if (value is long contentLength)
            {
                Response.Headers.Add("Content-Length", contentLength.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public Stream Body => Response.Body;
}


internal static class CoreFunctions
{
    public static Task InvokeCapabilities(HttpResponseData response, CancellationToken cancellationToken)
        => Images.Generic.CoreFunctions.InvokeCapabilities(
            new HttpResponseAdapter(response),
            cancellationToken
        );

    public static Task InvokeResize(
        HttpRequestData request,
        IResourceFactory resourceFactory,
        IImageResizer resizer,
        ILogger logger,
        CancellationToken cancellationToken)
        => Images.Generic.CoreFunctions.InvokeResize(
            new HttpRequestAdapter(request),
            resourceFactory,
            resizer,
            logger,
            cancellationToken
        );

    public static async Task<HttpResponseData> InvokeAnalyze(
        HttpRequestData request,
        IResourceFactory resourceFactory,
        IImageAnalyzer analyzer,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var response = request.CreateResponse();
        await Images.Generic.CoreFunctions.InvokeAnalyze(
            new HttpRequestAdapter(request),
            new HttpResponseAdapter(response),
            resourceFactory,
            analyzer,
            logger,
            cancellationToken
        ).ConfigureAwait(false);
        return response;
    }
}