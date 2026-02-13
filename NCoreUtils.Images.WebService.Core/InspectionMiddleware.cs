using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Images;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ImageResizerOptions))]
internal partial class ImageResizerOptionsSerializerContext : JsonSerializerContext { }

public class InspectionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly PathString _configEndpoint;

    public InspectionMiddleware(RequestDelegate next)
    {
        var prefix = Environment.GetEnvironmentVariable("ENDPOINT_PREFIX") switch
        {
            null => "/",
            "" => "/",
            var pre => $"/{pre.Trim('/')}/"
        };
        _configEndpoint = prefix + "config";
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        if (request.Path == _configEndpoint && StringComparer.OrdinalIgnoreCase.Equals(request.Method, "GET"))
        {
            var configuration = context.RequestServices.GetRequiredService<IImageResizerOptions>();
            if (configuration is ImageResizerOptions options)
            {
                var response = context.Response;
                response.StatusCode = StatusCodes.Status200OK;
                response.ContentType = "application/json";
                return JsonSerializer.SerializeAsync(response.Body, options, ImageResizerOptionsSerializerContext.Default.ImageResizerOptions);
            }
        }
        return _next(context);
    }
}