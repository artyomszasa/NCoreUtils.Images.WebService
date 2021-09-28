using System;

namespace NCoreUtils.Images
{
    public class AzureBlobStorageResourceFactory : IResourceFactory
    {
        public IImageDestination CreateDestination(Uri? uri, Func<IImageDestination> next)
        {
            if (uri is null || uri.Scheme != "az")
            {
                return next();
            }
            return new AzureBlobStorageDestination(uri);
        }

        public IImageSource CreateSource(Uri? uri, Func<IImageSource> next)
        {
            if (uri is null || uri.Scheme != "az")
            {
                return next();
            }
            return new AzureBlobStorageSource(uri);
        }
    }
}