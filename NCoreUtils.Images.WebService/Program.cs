using System;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .UseContentRoot(Environment.CurrentDirectory)
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
