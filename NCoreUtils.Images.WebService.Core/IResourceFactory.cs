using System;

namespace NCoreUtils.Images
{
    public interface IResourceFactory
    {
        IImageSource CreateSource(Uri? uri, Func<IImageSource> next);

        IImageDestination CreateDestination(Uri? uri, Func<IImageDestination> next);
    }
}