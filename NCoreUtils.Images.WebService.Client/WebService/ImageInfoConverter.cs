using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.Images.WebService
{
    internal class ImageInfoConverter : JsonConverter<ImageInfo>
    {
        public override ImageInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null || reader.TokenType == JsonTokenType.None)
            {
                return null!;
            }
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException("Expected object.");
            }
            int width = default;
            int height = default;
            int xResolution = default;
            int yResolution = default;
            var iptc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var exif = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            reader.ReadOrFail();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new InvalidOperationException("Expected property.");
                }
                if (reader.ValueTextEquals("width") || reader.ValueTextEquals("Width"))
                {
                    reader.ReadOrFail();
                    width = reader.GetInt32();
                    reader.ReadOrFail();
                }
                else if (reader.ValueTextEquals("height") || reader.ValueTextEquals("Height"))
                {
                    reader.ReadOrFail();
                    height = reader.GetInt32();
                    reader.ReadOrFail();
                }
                else if (reader.ValueTextEquals("xResolution") || reader.ValueTextEquals("XResolution"))
                {
                    reader.ReadOrFail();
                    xResolution = reader.GetInt32();
                    reader.ReadOrFail();
                }
                else if (reader.ValueTextEquals("yResolution") || reader.ValueTextEquals("YResolution"))
                {
                    reader.ReadOrFail();
                    yResolution = reader.GetInt32();
                    reader.ReadOrFail();
                }
                else if (reader.ValueTextEquals("iptc") || reader.ValueTextEquals("Iptc"))
                {
                    reader.ReadOrFail();
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new InvalidOperationException("Expected object.");
                    }
                    reader.ReadOrFail();
                    while (JsonTokenType.EndObject != reader.TokenType)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new InvalidOperationException("Expected property.");
                        }
                        var key = reader.GetString();
                        reader.ReadOrFail();
                        var value = reader.GetString();
                        iptc[key] = value;
                        reader.ReadOrFail();
                    }
                    reader.ReadOrFail();
                }
                else if (reader.ValueTextEquals("exif") || reader.ValueTextEquals("Exif"))
                {
                    reader.ReadOrFail();
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new InvalidOperationException("Expected object.");
                    }
                    reader.ReadOrFail();
                    while (JsonTokenType.EndObject != reader.TokenType)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new InvalidOperationException("Expected property.");
                        }
                        var key = reader.GetString();
                        reader.ReadOrFail();
                        var value = reader.GetString();
                        exif[key] = value;
                        reader.ReadOrFail();
                    }
                    reader.ReadOrFail();
                }
                else
                {
                    reader.ReadOrFail();
                    reader.Skip();
                    reader.ReadOrFail();
                }
            }
            reader.Read();
            return new ImageInfo(width, height, xResolution, yResolution, iptc, exif);
        }

        public override void Write(Utf8JsonWriter writer, ImageInfo value, JsonSerializerOptions options)
        {
            throw new InvalidOperationException($"{nameof(ImageInfoConverter)} not supposed to be used to serialize image info.");
        }
    }
}