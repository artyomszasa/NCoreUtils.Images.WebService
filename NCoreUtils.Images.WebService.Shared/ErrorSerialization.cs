using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Images.WebService
{
    internal static class ErrorSerialization
    {
        public static JsonSerializerOptions Options { get; }

        static ErrorSerialization()
        {
            Options = new JsonSerializerOptions();
            Options.Converters.Add(new ImageErrorConverter());
        }

        public static ValueTask<ImageErrorData?> DeserializeImageErrorDataAsync(System.IO.Stream stream, CancellationToken cancellationToken)
        {
            var typeInfo = (JsonTypeInfo<ImageErrorData>)Options.GetTypeInfo(typeof(ImageErrorData));
            return JsonSerializer.DeserializeAsync<ImageErrorData>(stream, typeInfo, cancellationToken);
        }

        public static Task SerializeImageErrorDataAsync(System.IO.Stream stream, ImageErrorData data, CancellationToken cancellationToken)
        {
            var typeInfo = (JsonTypeInfo<ImageErrorData>)Options.GetTypeInfo(typeof(ImageErrorData));
            return JsonSerializer.SerializeAsync(stream, data, typeInfo, cancellationToken);
        }

    }
}