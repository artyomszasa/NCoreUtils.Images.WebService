using System.Text;
using System.Text.Json;

namespace NCoreUtils.Images.WebService
{
    sealed class InternalImageErrorDeserializer : ErrorDeserializer, IErrorDeserializer
    {
        static readonly byte[] _keyInternalCode = Encoding.ASCII.GetBytes(ImageErrorProperties.InternalCode);

        string _internalCode = string.Empty;

        public InternalImageErrorDeserializer(string errorCode) : base(errorCode) { }

        public ImageErrorData CreateInstance()
            => new InternalImageErrorData(ErrorCode, Description, _internalCode);

        public void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.ValueTextEquals(_keyDescription))
            {
                reader.ReadOrFail();
                Description = reader.GetString();
                reader.ReadOrFail();
            }
            else if (reader.ValueTextEquals(_keyInternalCode))
            {
                reader.ReadOrFail();
                _internalCode = reader.GetString();
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