using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NCoreUtils.IO;

namespace NCoreUtils.Images
{
    public class HttpRequestSource : IImageSource
    {
        const int BufferSize = 16 * 1024;

        readonly HttpRequest _request;

        public bool Reusable => false;

        public HttpRequestSource(HttpRequest request)
            => _request = request ?? throw new ArgumentNullException(nameof(request));

        public IStreamProducer CreateProducer()
            => StreamProducer.Create((output, cancellationToken) => new ValueTask(_request.Body.CopyToAsync(output, BufferSize)));
    }
}