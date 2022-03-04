using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Images
{
    public class CompositeResourceFactory : IResourceFactory
    {
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067",
            Justification = "Types are preserved upon adding in CompositeResourceFactoryBuilder.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IResourceFactory ActivateFactory(IServiceProvider serviceProvider, Type type)
            => (IResourceFactory)ActivatorUtilities.CreateInstance(serviceProvider, type);

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
                .Select(type => ActivateFactory(serviceProvider, type))
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