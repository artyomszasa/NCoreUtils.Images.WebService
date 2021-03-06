using System.IO;
using System.Net.Http.Headers;

namespace NCoreUtils.Images.WebService
{
    class JsonStreamContent : TypedStreamContent
    {
        static readonly MediaTypeHeaderValue _applicationJson = MediaTypeHeaderValue.Parse("application/json");

        public JsonStreamContent(Stream stream) : base(stream, _applicationJson) { }
    }
}