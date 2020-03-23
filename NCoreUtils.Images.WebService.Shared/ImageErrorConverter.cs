using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.Images.WebService
{
    sealed class ImageErrorConverter : JsonConverter<ImageErrorData>
    {
        static readonly byte[] _keyErrorCode = Encoding.ASCII.GetBytes(ImageErrorProperties.ErrorCode);

        static readonly Dictionary<string, Func<string, IErrorDeserializer>> _deserializers = new Dictionary<string, Func<string, IErrorDeserializer>>
        {
            { ErrorCodes.InternalError, errorCode => new InternalImageErrorDeserializer(errorCode) },
            { ErrorCodes.UnsupportedImageType, errorCode => new UnsupportedImageTypeDeserializer(errorCode) },
            { ErrorCodes.UnsupportedResizeMode, errorCode => new UnsupportedResizeModeDeserializer(errorCode) }
        };

        public override ImageErrorData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                case JsonTokenType.Null:
                    return default!;
                case JsonTokenType.StartObject:
                    reader.ReadOrFail();
                    if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(_keyErrorCode))
                    {
                        reader.ReadOrFail();
                        var errorCode = reader.GetString();
                        var deserializer = _deserializers.TryGetValue(errorCode, out var factory)
                            ? factory(errorCode)
                            : new GenericErrorDeserializer(errorCode);
                        reader.ReadOrFail();
                        while (reader.TokenType != JsonTokenType.EndObject)
                        {
                            if (reader.TokenType == JsonTokenType.PropertyName)
                            {
                                deserializer.ReadProperty(ref reader, options);
                            }
                            else
                            {
                                throw new InvalidOperationException("Invalid json token encountered.");
                            }
                        }
                        return deserializer.CreateInstance();
                    }
                    throw new InvalidOperationException("First property must be an error_code.");
                default:
                    throw new InvalidOperationException("Invalid json token encountered.");
            }
        }

        public override void Write(Utf8JsonWriter writer, ImageErrorData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            value.WriteTo(writer);
            writer.WriteEndObject();
        }
    }
}