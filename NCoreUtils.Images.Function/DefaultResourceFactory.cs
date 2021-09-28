using System;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.Images
{
    public class DefaultResourceFactory : IResourceFactory
    {
        public IImageDestination CreateDestination(Uri? uri, Func<IImageDestination> next)
        {
            if (uri is null)
            {
                throw new NotSupportedException();
            }
            return next();
        }

        public IImageSource CreateSource(Uri? uri, Func<IImageSource> next)
        {
            if (uri is null)
            {
                throw new NotSupportedException();
            }
            return next();
        }
    }
}