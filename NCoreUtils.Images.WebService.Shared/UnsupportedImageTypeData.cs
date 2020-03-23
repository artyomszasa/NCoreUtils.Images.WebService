using System;
using System.Text.Json;

namespace NCoreUtils.Images.WebService
{
    [Serializable]
    public class UnsupportedImageTypeData : ImageErrorData
    {
        static readonly JsonEncodedText _keyImageType = JsonEncodedText.Encode(ImageErrorProperties.ImageType);

        public string ImageType { get; }

        public UnsupportedImageTypeData(string errorCode, string description, string imageType)
            : base(errorCode, description)
            => ImageType = imageType;

        internal override void WriteTo(Utf8JsonWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteString(_keyImageType, ImageType);
        }
    }
}