using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace BotApp.Luis.Router.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
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
                .UseLibuv()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();
    }
}