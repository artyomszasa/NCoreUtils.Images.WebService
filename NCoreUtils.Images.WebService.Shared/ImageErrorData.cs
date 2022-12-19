using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.Images.WebService;

[Serializable]
[JsonConverter(typeof(ImageErrorConverter))]
public class ImageErrorData
{
    static readonly JsonEncodedText _keyErrorCode = JsonEncodedText.Encode(ImageErrorProperties.ErrorCode);

    static readonly JsonEncodedText _keyDescription = JsonEncodedText.Encode(ImageErrorProperties.Description);

    public string ErrorCode { get; }

    public string Description { get; }

    public ImageErrorData(string errorCode, string description)
    {
        ErrorCode = errorCode;
        Description = description;
    }

    internal virtual void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteString(_keyErrorCode, ErrorCode);
        writer.WriteString(_keyDescription, Description);
    }
}