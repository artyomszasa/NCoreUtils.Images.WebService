using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images.WebService
{
    public class Startup
    {
        void ConfigureLogging(ILoggingBuilder builder)
            => builder
                .ClearProviders()
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole();

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
                .AddLogging(ConfigureLogging)
                .AddSingleton(imageResizerOptions)
                .AddSingleton(serviceConfiguration)
                .AddHttpContextAccessor()
                .AddHttpClient()
                .AddImageMagickResizer()
                .AddResourceFactories(b => b.Add<DefaultResourceFactory>().Add<GoogleCloudStorageResourceFactory>());
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
