using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace BotApp.Luis.Router.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((host, config) =>
                {
                    config.AddJsonFile("ocelot.json");
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .UseKestrel((options) =>
                {
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                    options.Limits.MaxRequestBodySize = 10 * 1024;
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 200, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 200, gracePeriod: TimeSpan.FromSeconds(10));
                })
                .UseUrls("https://*:443")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();
    }
}