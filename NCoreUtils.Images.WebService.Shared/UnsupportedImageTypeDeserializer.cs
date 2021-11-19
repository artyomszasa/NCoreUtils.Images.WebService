using System.Text;
using System.Text.Json;

namespace NCoreUtils.Images.WebService
{
    sealed class UnsupportedImageTypeDeserializer : ErrorDeserializer, IErrorDeserializer
    {
        static readonly byte[] _keyImageType = Encoding.ASCII.GetBytes(ImageErrorProperties.ImageType);

        string _imageType = string.Empty;

        public UnsupportedImageTypeDeserializer(string errorCode) : base(errorCode) { }

        public ImageErrorData CreateInstance()
            => new UnsupportedImageTypeData(ErrorCode, Description, _imageType);

        public void ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.ValueTextEquals(_keyDescription))
            {
                reader.ReadOrFail();
                Description = reader.GetString() ?? string.Empty;
                reader.ReadOrFail();
            }
            else if (reader.ValueTextEquals(_keyImageType))
            {
                reader.ReadOrFail();
                _imageType = reader.GetString() ?? string.Empty;
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