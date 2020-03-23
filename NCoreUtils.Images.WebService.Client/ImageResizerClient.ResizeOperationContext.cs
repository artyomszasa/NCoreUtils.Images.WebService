using System.Net.Http.Headers;
using NCoreUtils.IO;

namespace NCoreUtils.Images
{
    public partial class ImageResizerClient
    {
        protected sealed class ResizeOperationContext
        {
            static readonly MediaTypeHeaderValue _binary = MediaTypeHeaderValue.Parse("application/octet-stream");

            static readonly MediaTypeHeaderValue _json = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            public static ResizeOperationContext Inline(IStreamProducer producer, IImageDestination destination)
                => new ResizeOperationContext(_binary, producer, destination);

            public static ResizeOperationContext Json(IStreamProducer producer, IImageDestination? destination = default)
                => new ResizeOperationContext(_json, producer, destination);

            public MediaTypeHeaderValue ContentType { get; }

            public IStreamProducer Producer { get; }

            public IImageDestination? Destination { get; }

            ResizeOperationContext(MediaTypeHeaderValue contentType, IStreamProducer producer, IImageDestination? destination)
            {
                ContentType = contentType;
                Producer = producer;
                Destination = destination;
            }
        }
    }
}