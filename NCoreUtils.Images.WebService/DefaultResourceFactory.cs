using System;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.Images
{
    public class DefaultResourceFactory : IResourceFactory
    {
        readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultResourceFactory(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        public IImageDestination CreateDestination(Uri? uri, Func<IImageDestination> next)
        {
            if (uri is null)
            {
                return new HttpResponseDestination(_httpContextAccessor.HttpContext.Response);
            }
            return next();
        }

        public IImageSource CreateSource(Uri? uri, Func<IImageSource> next)
        {
            if (uri is null)
            {
                return new HttpRequestSource(_httpContextAccessor.HttpContext.Request);
            }
            return next();
        }
    }
}