using System.Text.Json.Serialization;

namespace NCoreUtils.Images.WebService;

[JsonSerializable(typeof(ImageErrorData))]
internal partial class ImageErrorSerializerContext : JsonSerializerContext { }