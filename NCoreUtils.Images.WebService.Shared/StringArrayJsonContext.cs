using System.Text.Json.Serialization;

namespace NCoreUtils.Images.WebService
{
    [JsonSerializable(typeof(string[]))]
    public partial class StringArrayJsonContext : JsonSerializerContext
    { }

}