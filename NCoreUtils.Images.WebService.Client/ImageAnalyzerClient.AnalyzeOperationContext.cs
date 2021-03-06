using System;
using System.Net.Http.Headers;
using NCoreUtils.IO;

namespace NCoreUtils.Images
{
    public partial class ImageAnalyzerClient
    {
        protected sealed class AnalyzeOperationContext
        {
            static readonly MediaTypeHeaderValue _binary = MediaTypeHeaderValue.Parse("application/octet-stream");

            static readonly MediaTypeHeaderValue _json = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            public static AnalyzeOperationContext Inline(IStreamProducer producer)
                => new AnalyzeOperationContext(_binary, producer);

            public static AnalyzeOperationContext Json(IStreamProducer producer)
                => new AnalyzeOperationContext(_json, producer);

            public MediaTypeHeaderValue ContentType { get; }

            public IStreamProducer Producer { get; }

            AnalyzeOperationContext(MediaTypeHeaderValue contentType, IStreamProducer producer)
            {
                ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
                Producer = producer ?? throw new ArgumentNullException(nameof(producer));
            }
        }
    }
}