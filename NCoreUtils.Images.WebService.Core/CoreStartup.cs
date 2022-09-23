using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace NCoreUtils.Images
{
    public abstract class CoreStartup
    {
        private sealed class ConfigureImageMagick : IConfigureOptions<ImageMagickImageProviderConfiguration>
        {
            public void Configure(ImageMagickImageProviderConfiguration options)
            {
                options.ForceAsync = true;
            }
        }

        protected IConfiguration Configuration { get; }

        protected IWebHostEnvironment? Env { get; }

        protected CoreStartup(IConfiguration configuration, IWebHostEnvironment? env)
        {
            Configuration = configuration;
            Env = env;
        }

        protected virtual void AddHttpContextAccessor(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor();
        }

        protected virtual IImageResizerOptions GetImageResizerOptions()
        {
            var section = Configuration.GetSection("Images");
            var options = new ImageResizerOptions();
            var rawMemoryLimit = section[nameof(ImageResizerOptions.MemoryLimit)];
            if (rawMemoryLimit is not null)
            {
                if (long.TryParse(rawMemoryLimit, NumberStyles.Integer, CultureInfo.InvariantCulture, out var memoryLimit))
                {
                    options.MemoryLimit = memoryLimit;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid value for Images:{nameof(ImageResizerOptions.MemoryLimit)}: \"{rawMemoryLimit}\".");
                }
            }
            foreach (var (key, value) in section.GetSection(nameof(ImageResizerOptions.Quality)).AsEnumerable())
            {
                if (value is not null)
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ivalue))
                    {
                        options.Quality[key] = ivalue;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid value for Images:{nameof(ImageResizerOptions.Quality)}:{key}: \"{value}\".");
                    }
                }
            }
            foreach (var (key, value) in section.GetSection(nameof(ImageResizerOptions.Optimize)).AsEnumerable())
            {
                if (value is not null)
                {
                    if (bool.TryParse(value, out var bvalue))
                    {
                        options.Optimize[key] = bvalue;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid value for Images:{nameof(ImageResizerOptions.Optimize)}:{key}: \"{value}\".");
                    }
                }
            }
            return options;
        }

        protected virtual ServiceConfiguration GetServiceConfiguration()
        {
            var section = Configuration.GetSection("Images");
            var configuration = new ServiceConfiguration();
            var rawMaxConcurrentOps = section[nameof(ServiceConfiguration.MaxConcurrentOps)];
            if (rawMaxConcurrentOps is not null)
            {
                if (int.TryParse(rawMaxConcurrentOps, NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxConcurrentOps))
                {
                    configuration.MaxConcurrentOps = maxConcurrentOps;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid value for Images:{nameof(ServiceConfiguration.MaxConcurrentOps)}: \"{rawMaxConcurrentOps}\".");
                }
            }
            return configuration;
        }

        protected abstract void ConfigureResourceFactories(CompositeResourceFactoryBuilder b);

        public virtual void ConfigureServices(IServiceCollection services)
        {
            AddHttpContextAccessor(services);
            services
                // JSON Serialization
                .AddOptions<JsonSerializerOptions>()
                    .Configure(opts => opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                    .Services
                // image resizer options
                .AddSingleton(GetImageResizerOptions())
                // service options
                .AddSingleton(GetServiceConfiguration())
                // HTTP client factory
                .AddHttpClient()
                // ImageMagick
                .AddImageMagickResizer()
                .ConfigureOptions<ConfigureImageMagick>()
                // source/destination handlers
                .AddResourceFactories(ConfigureResourceFactories);
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            #if DEBUG
            if (Env is not null && Env.IsDevelopment())
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