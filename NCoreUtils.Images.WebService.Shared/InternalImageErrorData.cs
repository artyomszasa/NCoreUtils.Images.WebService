using System;
using System.Text.Json;

namespace NCoreUtils.Images.WebService
{
    [Serializable]
    public class InternalImageErrorData : ImageErrorData
    {
        static readonly JsonEncodedText _keyInternalCode = JsonEncodedText.Encode(ImageErrorProperties.InternalCode);

        public string InternalCode { get; }

        public InternalImageErrorData(string errorCode, string description, string internalCode)
            : base(errorCode, description)
            => InternalCode = internalCode;

        internal override void WriteTo(Utf8JsonWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteString(_keyInternalCode, InternalCode);
        }
    }
}