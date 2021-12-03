using System;

namespace NCoreUtils.Images
{
    public class FileSystemResourceFactory : IResourceFactory
    {
        public IImageDestination CreateDestination(Uri? uri, Func<IImageDestination> next)
        {
            if (uri is not null && uri.Scheme == "file")
            {
                return new FileSystemDestination(uri.AbsolutePath);
            }
            return next();
        }

        public IImageSource CreateSource(Uri? uri, Func<IImageSource> next)
        {
            if (uri is not null && uri.Scheme == "file")
            {
                return new FileSystemSource(uri.AbsolutePath);
            }
            return next();
        }
    }
}