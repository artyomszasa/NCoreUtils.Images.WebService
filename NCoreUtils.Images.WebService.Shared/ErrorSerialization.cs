using System.Text.Json;

namespace NCoreUtils.Images.WebService
{
    internal static class ErrorSerialization
    {
        public static JsonSerializerOptions Options { get; }

        static ErrorSerialization()
        {
            Options = new JsonSerializerOptions();
            Options.Converters.Add(new ImageErrorConverter());
        }
    }
}