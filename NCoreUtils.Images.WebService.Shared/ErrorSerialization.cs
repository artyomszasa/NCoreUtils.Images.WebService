using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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

#if !NETSTANDARD2_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Potentially serializable types referenced directly within converter.")]
#endif
        public static ValueTask<ImageErrorData?> DeserializeImageErrorDataAsync(System.IO.Stream stream, CancellationToken cancellationToken)
            => JsonSerializer.DeserializeAsync<ImageErrorData>(stream, ErrorSerialization.Options, cancellationToken);

#if !NETSTANDARD2_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Potentially serializable types referenced directly within converter.")]
#endif
        public static Task SerializeImageErrorDataAsync(System.IO.Stream stream, ImageErrorData data, CancellationToken cancellationToken)
            => JsonSerializer.SerializeAsync(stream, data, ErrorSerialization.Options, cancellationToken);

    }
}