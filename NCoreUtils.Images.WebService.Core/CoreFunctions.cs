using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images;

public static class CoreFunctions
{
    private static byte[] Capabilities { get; } = JsonSerializer.SerializeToUtf8Bytes(
        new [] { WebService.Capabilities.JsonSerializedImageInfo },
        StringArrayJsonContext.Default.StringArray
    );

    private static HashSet<string> Truthy { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "true",
        "t",
        "on",
        "1"
    };

    [DoesNotReturn]
    internal static void NotSupportedUri(Uri? uri)
        => throw new ImageException("unsupported_uri", $"Either invalid or unsupported uri: {uri}.");

    private static bool IsJsonCompatible(string? contentType)
        => contentType is not null &&
            (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
                || contentType.StartsWith("text/json", StringComparison.OrdinalIgnoreCase)
                || contentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase));

    private static ResizeOptions ReadResizeOptions(IResourceFactory resourceFactory, IQueryCollection query)
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
            filters: FilterParser.Parse(resourceFactory, S("f"))
        );

        bool? B(string name)
        {
            return S(name) switch
            {
                null => default,
                string s => Truthy.Contains(s)
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
            return JsonSerializer.DeserializeAsync(
                request.Body,
                SourceAndDestinationJsonContext.Default.SourceAndDestination,
                cancellationToken
            );
        }
        return default;
    }

    private static (IReadableResource Source, IWritableResource Destination) ResolveSourceAndDestination(IResourceFactory resourceFactory, SourceAndDestination sd)
    {
        if (!resourceFactory.TryCreateReadable(sd.Source!, out var source))
        {
            NotSupportedUri(sd.Source);
        }
        if (!resourceFactory.TryCreateWritable(sd.Destination!, out var destination))
        {
            NotSupportedUri(sd.Destination);
        }
        return (source, destination);
    }

    public static async Task InvokeCapabilities(HttpResponse response, CancellationToken cancellationToken)
    {
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength = Capabilities.Length;
        await response.BodyWriter.WriteAsync(Capabilities, cancellationToken).ConfigureAwait(false);
        await response.BodyWriter.CompleteAsync().ConfigureAwait(false);
    }

    public static async Task InvokeResize(HttpRequest request, IResourceFactory resourceFactory, IImageResizer resizer, CancellationToken cancellationToken)
    {
        var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
        var (source, destination) = ResolveSourceAndDestination(resourceFactory, sourceAndDestination);
        await resizer.ResizeAsync(source, destination, ReadResizeOptions(resourceFactory, request.Query), cancellationToken).ConfigureAwait(false);
    }

    public static async ValueTask<ImageInfo> InvokeAnalyze(HttpRequest request, IResourceFactory resourceFactory, IImageAnalyzer analyzer, CancellationToken cancellationToken)
    {
        var sourceAndDestination = await ParseSourceAndDestination(request, cancellationToken).ConfigureAwait(false);
        if (!resourceFactory.TryCreateReadable(sourceAndDestination.Source!, out var source))
        {
            NotSupportedUri(sourceAndDestination.Source);
        }
        return await analyzer.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
    }

    public static async Task InvokeAnalyze(HttpRequest request, HttpResponse response, IResourceFactory resourceFactory, IImageAnalyzer analyzer, CancellationToken cancellationToken)
    {
        var info = await InvokeAnalyze(request, resourceFactory, analyzer, cancellationToken).ConfigureAwait(false);
        response.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(response.Body, info, ImageInfoJsonContext.Default.ImageInfo, cancellationToken)
            .ConfigureAwait(false);
        await response.Body.FlushAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}