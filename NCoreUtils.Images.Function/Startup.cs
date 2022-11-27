using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NCoreUtils.Resources;

[assembly: FunctionsStartup(typeof(NCoreUtils.Images.Startup))]

namespace NCoreUtils.Images
{
    public class Startup : FunctionsStartup
    {
        private sealed class InternalStartup : CoreStartup
        {
            public InternalStartup(IConfiguration configuration) : base(configuration, default) { }

            protected override void AddHttpContextAccessor(IServiceCollection services) { }

            protected override void ConfigureResourceFactories(OptionsBuilder<CompositeResourceFactoryConfiguration> b)
            {
                b
                    // Azure Bloc Storage
                    .AddAzureBlobResourceFactory();
            }
        }

        private static IConfigurationRoot LoadConfiguration()
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: false)
                .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var internalStartup = new InternalStartup(LoadConfiguration());
            internalStartup.ConfigureServices(builder.Services);
        }
    }
}
