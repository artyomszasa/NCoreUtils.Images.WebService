using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NCoreUtils.Resources;

namespace NCoreUtils.Images.WebService
{
    public class Startup(IConfiguration configuration, IWebHostEnvironment env) : CoreStartup(configuration, env)
    {
        protected override void ConfigureResourceFactories(OptionsBuilder<CompositeResourceFactoryConfiguration> b)
        {
            b
#if EnableGoogleCloudStorage
                // GCS
                .AddGoogleCloudStorageResourceFactory(passthrough: false)
#endif
#if EnableAzureBlobStorage
                // Azure Bloc Storage
                .AddAzureBlobResourceFactory()
#endif
#if EnableLocalStorage
                // locally mounted fs
                .AddFileSystemResourceFactory()
#endif
                ;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
#if EnableGoogleCloudStorage
            services.AddGoogleCloudStorageUtils();
#endif
        }
    }
}
