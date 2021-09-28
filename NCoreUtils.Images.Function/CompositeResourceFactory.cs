using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images
{
    public class CompositeResourceFactory : IResourceFactory
    {
        private ILogger _logger;

        public IReadOnlyList<IResourceFactory> Factories { get; }

        public CompositeResourceFactory(IServiceProvider serviceProvider, CompositeResourceFactoryBuilder builder)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            _logger = serviceProvider.GetRequiredService<ILogger<CompositeResourceFactory>>();
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
                _logger.LogInformation("Trying source factory #{0}.", index);
                if (index < Factories.Count)
                {
                    _logger.LogInformation("Trying source factory {0}.", Factories[index].ToString());
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
                _logger.LogInformation("Trying destination factory #{0}.", index);
                if (index < Factories.Count)
                {
                    _logger.LogInformation("Trying destination factory {0}.", Factories[index].ToString());
                    return Factories[index].CreateDestination(uri, () => DoCreateDestination(index + 1));
                }
                return next();
            }
        }
    }
}