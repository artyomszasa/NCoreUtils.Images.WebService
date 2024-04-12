using System;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#if EnableGoogleFluentdLogging
using NCoreUtils.Logging;
#endif

namespace NCoreUtils.Images.WebService;

public class Program
{
    private static IConfigurationRoot LoadConfiguration()
        => new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddEnvironmentVariables("IMAGES_")
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: false)
            .Build();


    private static IPEndPoint ParseEndpoint(string? input)
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
            return new IPEndPoint(IPAddress.Parse(input.AsSpan(0, portIndex)), int.Parse(input.AsSpan()[(portIndex + 1)..]));
        }
    }

    private static void ConfigureKestrel(KestrelServerOptions options)
    {
        // Google Cloud Run passes port to listen on through PORT env variable.
        var endpoint = Environment.GetEnvironmentVariable("PORT") switch
        {
            null => ParseEndpoint(Environment.GetEnvironmentVariable("LISTEN")),
            string portAsString => int.TryParse(portAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port)
                ? new IPEndPoint(IPAddress.Any, port)
                : ParseEndpoint(Environment.GetEnvironmentVariable("LISTEN"))
        };
        options.Listen(endpoint);
        // options.AllowSynchronousIO = true;
    }

    private static void ConfigureLogging(ILoggingBuilder builder, IConfiguration configuration)
    {
        builder
            .ClearProviders()
            .AddConfiguration(configuration.GetSection("Logging"))
#if EnableGoogleFluentdLogging
            .AddGoogleFluentd<AspNetCoreLoggerProvider>(projectId: configuration["Google:ProjectId"]);
#else
            .AddConsole();
#endif
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") switch
            {
                null or "" => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") switch
                {
                    null or "" => "Development",
                    string dotnetEnv => dotnetEnv
                },
                string aspNetCoreEnv => aspNetCoreEnv
            },
            ContentRootPath = Environment.CurrentDirectory
        });
        builder.Host.UseConsoleLifetime();
        var configuration = LoadConfiguration();
        builder.Configuration.AddConfiguration(configuration);
        // app.Logging.ConfigureLogging(configuration);
        ConfigureLogging(builder.Logging, configuration);
        builder.WebHost.UseKestrel(ConfigureKestrel);
        var startup = new Startup(configuration, builder.Environment);
        startup.ConfigureServices(builder.Services);
        // *************************************************************************************************************
        var app = builder.Build();
        startup.Configure(app);
        app.Start();
        app.WaitForShutdown();
    }
}