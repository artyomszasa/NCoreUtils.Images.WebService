using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images
{
    public partial class ImagesMiddleware
    {
        static byte[] _capabilities = JsonSerializer.SerializeToUtf8Bytes(new [] { Capabilities.JsonSerializedImageInfo });

        static readonly JsonSerializerOptions _sourceAndDestinationSerializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { SourceAndDestinationConverter.Instance }
        };

        static readonly HashSet<string> _truthy = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "true",
            "t",
            "on",
            "1"
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static bool Eqi(string a, string b)
            => StringComparer.OrdinalIgnoreCase.Equals(a, b);

        // [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        // static (int, int) GetSegment(in ReadOnlySpan<char> source)
        // {
        //     var startIndex = 0;
        //     while (startIndex < source.Length && '/' == source[startIndex])
        //     {
        //         ++startIndex;
        //     }
        //     var endIndex = source.Length - 1;
        //     while (endIndex > 0 && '/' == source[endIndex])
        //     {
        //         --endIndex;
        //     }
        //     return (startIndex, endIndex - startIndex + 1); // source.Slice(startIndex, endIndex - startIndex + 1);
        // }

        static T NotSupportedUri<T>(Uri? uri)
            => throw new ImageException("unsupported_uri", $"Either invalid or unsupported uri: {uri}.");

        static bool IsJsonCompatible(string contentType)
            => contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
                || contentType.StartsWith("text/json", StringComparison.OrdinalIgnoreCase)
                || contentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase);

        static ResizeOptions ReadResizeOptions(IQueryCollection query)
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



        ValueTask<SourceAndDestination> ParseSourceAndDestination(HttpRequest request, CancellationToken cancellationToken)
        {
            if (IsJsonCompatible(request.ContentType))
            {
                return JsonSerializer.DeserializeAsync<SourceAndDestination>(request.Body, _sourceAndDestinationSerializationOptions, cancellationToken);
            }
            return default;
        }

        (IImageSource Source, IImageDestination Destination) ResolveSourceAndDestination(IResourceFactory resourceFactory, SourceAndDestination sd)
        {
            var source = resourceFactory.CreateSource(sd.Source, () => NotSupportedUri<IImageSource>(sd.Source));
            var destination = resourceFactory.CreateDestination(sd.Destination, () => NotSupportedUri<IImageDestination>(sd.Destination));
            return (source, destination);
        }
    }
}