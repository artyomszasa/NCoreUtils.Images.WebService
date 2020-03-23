using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Images
{
    public class CompositeResourceFactory : IResourceFactory
    {
        public IReadOnlyList<IResourceFactory> Factories { get; }

        public CompositeResourceFactory(IServiceProvider serviceProvider, CompositeResourceFactoryBuilder builder)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            Factories = builder.Factories
                .Select(type => (IResourceFactory)ActivatorUtilities.CreateInstance(serviceProvider, type))
                .ToArray();
        }

        public IImageSource CreateSource(Uri? uri, Func<IImageSource> next)
        {
            return DoCreateSource(0);

            IImageSource DoCreateSource(int index)
            {
                if (index < Factories.Count)
                {
                    return Factories[index].CreateSource(uri, () => DoCreateSource(index + 1));
                }
                return next();
            }
        }

        public IImageDestination CreateDestination(Uri? uri, Func<IImageDestination> next)
        {
            return DoCreateDestination(0);

            IImageDestination DoCreateDestination(int index)
            {
                if (index < Factories.Count)
                {
                    return Factories[index].CreateDestination(uri, () => DoCreateDestination(index + 1));
                }
                return next();
            }
        }
    }
}