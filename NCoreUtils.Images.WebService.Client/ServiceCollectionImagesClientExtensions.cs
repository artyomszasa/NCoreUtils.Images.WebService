using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Images;

public static class ServiceCollectionImagesClientExtensions
{
    private static void BindImagesClientConfiguration(this IConfiguration configuration, ImagesClientConfiguration conf)
    {
        if (configuration[nameof(conf.EndPoint)] is string endpoint)
        {
            conf.EndPoint = endpoint;
        }
        if (configuration[nameof(conf.AllowInlineData)] is string rawAllowInlineData)
        {
            conf.AllowInlineData = StringComparer.InvariantCultureIgnoreCase.Equals("true", rawAllowInlineData)
                ? true
                : StringComparer.InvariantCultureIgnoreCase.Equals("false", rawAllowInlineData)
                    ? false
                    : throw new InvalidOperationException($"Invalid configuration value for ImagesClientConfiguration.AllowInlineData: \"{rawAllowInlineData}\".");
        }
        if (configuration[nameof(conf.CacheCapabilities)] is string rawCacheCapabilities)
        {
            conf.AllowInlineData = StringComparer.InvariantCultureIgnoreCase.Equals("true", rawCacheCapabilities)
                ? true
                : StringComparer.InvariantCultureIgnoreCase.Equals("false", rawCacheCapabilities)
                    ? false
                    : throw new InvalidOperationException($"Invalid configuration value for ImagesClientConfiguration.CacheCapabilities: \"{rawCacheCapabilities}\".");
        }
        if (configuration[nameof(conf.HttpClient)] is string httpClient)
        {
            conf.HttpClient = httpClient;
        }
    }

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

    public static IServiceCollection AddImageResizerClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        var conf = new ImagesClientConfiguration();
        configuration.BindImagesClientConfiguration(conf);
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

    public static IServiceCollection AddImageAnalyzerClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        var conf = new ImagesClientConfiguration();
        configuration.BindImagesClientConfiguration(conf);
        return services.AddImageAnalyzerClient(conf);
    }
}