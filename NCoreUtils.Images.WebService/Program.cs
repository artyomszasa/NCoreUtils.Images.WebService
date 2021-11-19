using System;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Images.WebService
{
    public class Program
    {
        static IPEndPoint ParseEndpoint(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new IPEndPoint(IPAddress.Loopback, 5000);
            }
            var portIndex = input.LastIndexOf(':');
            if (-1 == portIndex)
            {
                return new IPEndPoint(IPAddress.Parse(input), 5000);
            }
            else
            {
                return new IPEndPoint(IPAddress.Parse(input.Substring(0, portIndex)), int.Parse(input.Substring(portIndex + 1)));
            }
        }

        private static IConfigurationRoot LoadConfiguration()
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: false)
                .Build();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = LoadConfiguration();
            return new HostBuilder()
                .UseContentRoot(Environment.CurrentDirectory)
                .ConfigureAppConfiguration(b => b.AddConfiguration(configuration))
                .ConfigureLogging((ctx, builder) =>
                {
                    builder
                        .ClearProviders()
                        .AddConfiguration(configuration)
                        .AddConsole();
                })
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o =>
                    {
                        // Google Cloud Run passes port to listen on through PORT env variable.
                        var endpoint = Environment.GetEnvironmentVariable("PORT") switch
                        {
                            null => ParseEndpoint(Environment.GetEnvironmentVariable("LISTEN")),
                            string portAsString => int.TryParse(portAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port)
                                ? new IPEndPoint(IPAddress.Any, port)
                                : ParseEndpoint(Environment.GetEnvironmentVariable("LISTEN"))
                        };
                        o.Listen(endpoint);
                        o.AllowSynchronousIO = true;
                    });
                });
        }
    }
}
