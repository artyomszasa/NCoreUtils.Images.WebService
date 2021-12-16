using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Images
{
    public static class ServiceCollectionImagesClientExtensions
    {
        public static IServiceCollection AddImageResizerClient(
            this IServiceCollection services,
            ImagesClientConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (string.IsNullOrEmpty(configuration.EndPoint))
            {
                throw new InvalidOperationException("Image resizer client endpoint must not be empty");
            }
            return services
                .AddSingleton(configuration.AsTyped<ImageResizerClient>())
                .AddSingleton<IImageResizer, ImageResizerClient>();
        }

        public static IServiceCollection AddImageResizerClient(
            this IServiceCollection services,
            string endpoint,
            bool allowInlineData = false,
            bool cacheCapabilities = true,
            string httpClient = ImagesClientConfiguration.DefaultHttpClient)
            => services.AddImageResizerClient(new ImagesClientConfiguration
            {
                EndPoint = endpoint,
                AllowInlineData = allowInlineData,
                CacheCapabilities = cacheCapabilities,
                HttpClient = httpClient
            });

#if !NETSTANDARD2_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Dynamic dependency binds required members.")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ImagesClientConfiguration))]
#endif
        public static IServiceCollection AddImageResizerClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var conf = new ImagesClientConfiguration();
            configuration.Bind(conf);
            return services.AddImageResizerClient(conf);
        }

        public static IServiceCollection AddImageAnalyzerClient(
            this IServiceCollection services,
            ImagesClientConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (string.IsNullOrEmpty(configuration.EndPoint))
            {
                throw new InvalidOperationException("Image analyzer client endpoint must not be empty");
            }
            return services
                .AddSingleton(configuration.AsTyped<ImageAnalyzerClient>())
                .AddSingleton<IImageAnalyzer, ImageAnalyzerClient>();
        }

        public static IServiceCollection AddImageAnalyzerClient(
            this IServiceCollection services,
            string endpoint,
            bool allowInlineData = false,
            bool cacheCapabilities = true,
            string httpClient = ImagesClientConfiguration.DefaultHttpClient)
            => services.AddImageAnalyzerClient(new ImagesClientConfiguration
            {
                EndPoint = endpoint,
                AllowInlineData = allowInlineData,
                CacheCapabilities = cacheCapabilities,
                HttpClient = httpClient
            });

#if !NETSTANDARD2_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Dynamic dependency binds required members.")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ImagesClientConfiguration))]
#endif
        public static IServiceCollection AddImageAnalyzerClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var conf = new ImagesClientConfiguration();
            configuration.Bind(conf);
            return services.AddImageAnalyzerClient(conf);
        }
    }
}