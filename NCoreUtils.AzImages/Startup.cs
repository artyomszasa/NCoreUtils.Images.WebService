using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NCoreUtils.Images;
using NCoreUtils.Resources;

namespace NCoreUtils.AzImages;

internal class Startup(IConfiguration configuration)
    : Images.Generic.CoreStartup(configuration)
{
    private sealed class ConfigureImageMagick : IConfigureOptions<ImageMagickImageProviderConfiguration>
    {
        public void Configure(ImageMagickImageProviderConfiguration options)
        {
            options.ForceAsync = true;
            options.EnableLogging = true;
            options.EnableResourceTracking = true;
        }
    }

    protected override void ConfigureResourceFactories(OptionsBuilder<CompositeResourceFactoryConfiguration> b)
    {
        b
            // Azure Blob Storage
            .AddAzureBlobResourceFactory();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.ConfigureOptions<ConfigureImageMagick>();
        base.ConfigureServices(services);
    }
}