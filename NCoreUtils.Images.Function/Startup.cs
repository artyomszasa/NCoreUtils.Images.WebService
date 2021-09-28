using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images
{
    public class Startup
    {
        private sealed class LoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public LoggerProvider(ILogger logger)
                => _logger = logger;

            public ILogger CreateLogger(string categoryName)
                => _logger;

            public void Dispose() { }
        }

        private sealed class Logger<T> : ILogger<T>
        {
            private readonly ILogger _logger;

            public Logger(ILoggerProvider loggerProvider)
                => _logger = loggerProvider.CreateLogger(typeof(T).Name);

            public IDisposable BeginScope<TState>(TState state)
                => _logger.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel)
                => _logger.IsEnabled(logLevel);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => _logger.Log(logLevel, eventId, state, exception, formatter);
                // => _logger.Log(logLevel < LogLevel.Information ? LogLevel.Information : logLevel, eventId, state, exception, formatter);
        }

        public void ConfigureServices(IServiceCollection services, ILogger logger)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var imageResizerOptions = configuration.GetSection("Images")
                .Get<ImageResizerOptions>()
                ?? ImageResizerOptions.Default;

            var serviceConfiguration = configuration.GetSection("Images")
                .Get<ServiceConfiguration>()
                ?? new ServiceConfiguration();

            services
                // JSON serialization
                .AddOptions<JsonSerializerOptions>()
                    .Configure(opts => opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                    .Services
                // Logging
                .AddSingleton<ILoggerProvider>(new LoggerProvider(logger))
                .AddTransient(typeof(ILogger<>), typeof(Logger<>))
                // image resizer options
                .AddSingleton(imageResizerOptions)
                // service options
                .AddSingleton(serviceConfiguration)
                // HTTP client factory
                .AddHttpClient()
                // ImageMagick
                .AddImageMagickResizer()
                // source/destination handlers
                .AddResourceFactories(b => b
                    // inline data
                    .Add<DefaultResourceFactory>()
                    // GCS
                    .Add<GoogleCloudStorageResourceFactory>()
                    // Azure Bloc Storage
                    .Add<AzureBlobStorageResourceFactory>()
                );
        }
    }
}
