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
                var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Unable to access current HTTP context.");
                return new HttpResponseDestination(context.Response);
            }
            return next();
        }

        public IImageSource CreateSource(Uri? uri, Func<IImageSource> next)
        {
            if (uri is null)
            {
                var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Unable to access current HTTP context.");
                return new HttpRequestSource(context.Request);
            }
            return next();
        }
    }
}