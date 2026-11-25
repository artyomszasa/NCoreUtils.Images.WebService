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
#if EnableR2Storage
                // Cloudflare R2 (r2://bucket/key) through the JSON based S3 proxy
                .AddS3ProxyResourceFactory()
#endif
                ;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
#if EnableGoogleCloudStorage
            services.AddGoogleCloudStorageUtils();
#endif
#if EnableR2Storage
            // S3 proxy endpoint, e.g. IMAGES_S3Proxy__Endpoint environment variable
            services.AddS3StorageClient(Configuration["S3Proxy:Endpoint"] switch
            {
                null or "" => throw new System.InvalidOperationException("No S3 proxy endpoint configured (S3Proxy:Endpoint)."),
                var endpoint => endpoint
            });
#endif
        }
    }
}
