using System.Text.Json;

namespace NCoreUtils.Images.WebService
{
    public interface IErrorDeserializer
    {
        ImageErrorData CreateInstance();

        void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options);
    }
}