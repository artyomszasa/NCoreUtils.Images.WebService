using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
// using NCoreUtils.AspNetCore;

namespace NCoreUtils.Images.WebService
{
    public class Startup
    {
        void ConfigureLogging(ILoggingBuilder builder, IConfiguration configuration, string? projectId = default)
            => builder
                .ClearProviders()
                .AddConfiguration(configuration)
                .AddConsole();
                // .AddGoogleFluentdSink(projectId: projectId, categoryHandling: CategoryHandling.IncludeAsLabel, eventIdHandling: EventIdHandling.Ignore);

        public void ConfigureServices(IServiceCollection services)
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
                // JSON Serialization
                .AddOptions<JsonSerializerOptions>()
                    .Configure(opts => opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                    .Services
                // Logging
                .AddLogging(b => ConfigureLogging(b, configuration.GetSection("Logging"), configuration["Google:ProjectId"]))
                // image resizer options
                .AddSingleton(imageResizerOptions)
                // service options
                .AddSingleton(serviceConfiguration)
                // HTTP context accessor
                .AddHttpContextAccessor()
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
                    .Add<AzureBlocStorageResourceFactory>()
                    // locally mounted fs
                    .Add<FileSystemResourceFactory>()
                );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #if DEBUG
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            #endif

            app
                // .UsePrePopulateLoggingContext()
                .UseMiddleware<ErrorMiddleware>()
                .UseMiddleware<ImagesMiddleware>()
                .Run((context) =>
                {
                    context.Response.StatusCode = 404;
                    return Task.CompletedTask;
                });
        }
    }
}
