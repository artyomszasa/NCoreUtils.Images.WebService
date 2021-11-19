using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace NCoreUtils.Images.WebService
{
    public class Startup : CoreStartup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env) { }

        protected override void ConfigureResourceFactories(CompositeResourceFactoryBuilder b)
        {
            b
                // inline data
                .Add<DefaultResourceFactory>()
                // GCS
                .Add<GoogleCloudStorageResourceFactory>();
        }
    }
}
