using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NCoreUtils.Resources;

namespace NCoreUtils.Images;

public abstract class CoreStartup : Generic.CoreStartup
{
    private sealed class ConfigureImageMagick : IConfigureOptions<ImageMagickImageProviderConfiguration>
    {
        public void Configure(ImageMagickImageProviderConfiguration options)
        {
            options.ForceAsync = true;
        }
    }

    protected IHostEnvironment? Env { get; }

    protected CoreStartup(IConfiguration configuration, IHostEnvironment? env)
        : base(configuration)
    {
        Env = env;
    }

    protected virtual void AddHttpContextAccessor(IServiceCollection services)
    {
        services
            .AddHttpContextAccessor();
    }

    protected override void ConfigureResourceFactories(OptionsBuilder<CompositeResourceFactoryConfiguration> b)
    {
        b.AddAspNetCoreResourceFactory();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        AddHttpContextAccessor(services);
        services
            .AddHttpClient()
            .ConfigureOptions<ConfigureImageMagick>();
        base.ConfigureServices(services);
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
            .UseMiddleware<InspectionMiddleware>()
            .Run((context) =>
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            });
    }
}