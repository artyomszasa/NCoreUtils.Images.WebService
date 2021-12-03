using System.Text.Json.Serialization;

namespace NCoreUtils.Images.WebService
{
    [JsonSerializable(typeof(SourceAndDestination))]
    public partial class SourceAndDestinationJsonContext : JsonSerializerContext { }
}