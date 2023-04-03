using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Images.WebService;

internal static class ErrorSerialization
{
    public static ValueTask<ImageErrorData?> DeserializeImageErrorDataAsync(System.IO.Stream stream, CancellationToken cancellationToken)
        => JsonSerializer.DeserializeAsync<ImageErrorData>(stream, ImageErrorSerializerContext.Default.ImageErrorData, cancellationToken);

    public static Task SerializeImageErrorDataAsync(System.IO.Stream stream, ImageErrorData data, CancellationToken cancellationToken)
        => JsonSerializer.SerializeAsync(stream, data, ImageErrorSerializerContext.Default.ImageErrorData, cancellationToken);
}