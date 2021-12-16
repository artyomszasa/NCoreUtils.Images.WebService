using System.Diagnostics.CodeAnalysis;
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

#if !NETCOREAPP3_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Dynamic dependency binds required members.")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ImageResizerOptions))]
#endif
        protected virtual IImageResizerOptions GetImageResizerOptions()
            => Configuration.GetSection("Images")
                .Get<ImageResizerOptions>()
                ?? ImageResizerOptions.Default;

#if !NETCOREAPP3_1
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Dynamic dependency binds required members.")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ServiceConfiguration))]
#endif
        protected virtual ServiceConfiguration GetServiceConfiguration()
            => Configuration.GetSection("Images")
                .Get<ServiceConfiguration>()
                ?? new ServiceConfiguration();

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