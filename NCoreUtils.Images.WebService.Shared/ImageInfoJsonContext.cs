using System.Text.Json.Serialization;

namespace NCoreUtils.Images.WebService
{
    [JsonSerializable(typeof(ImageInfo))]
    public partial class ImageInfoJsonContext : JsonSerializerContext
    {

    }
}