using System.Text.Json;

namespace NCoreUtils.Images.WebService
{
    class GenericErrorDeserializer : ErrorDeserializer, IErrorDeserializer
    {
        public GenericErrorDeserializer(string errorCode) : base(errorCode) { }

        public ImageErrorData CreateInstance()
            => new ImageErrorData(ErrorCode, Description);

        public void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.ValueTextEquals(_keyDescription))
            {
                reader.ReadOrFail();
                Description = reader.GetString();
                reader.ReadOrFail();
            }
            else
            {
                reader.ReadOrFail();
                reader.Skip();
            }
        }
    }
}