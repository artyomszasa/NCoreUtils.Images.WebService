using System.IO;
using System.Net.Http.Headers;

namespace NCoreUtils.Images.WebService;

class JsonStreamContent(Stream stream) : TypedStreamContent(stream, _applicationJson)
{
    static readonly MediaTypeHeaderValue _applicationJson = MediaTypeHeaderValue.Parse("application/json");
}