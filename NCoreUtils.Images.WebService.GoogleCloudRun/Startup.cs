using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NCoreUtils.Resources;

namespace NCoreUtils.Images.WebService
{
    public class Startup : CoreStartup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env) { }

        protected override void ConfigureResourceFactories(OptionsBuilder<CompositeResourceFactoryConfiguration> b)
        {
            b
                // GCS
                .AddGoogleCloudStorageResourceFactory(passthrough: false);
        }
    }
}
