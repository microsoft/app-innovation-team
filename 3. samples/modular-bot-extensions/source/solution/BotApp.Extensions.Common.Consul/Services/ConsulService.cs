using BotApp.Extensions.Common.Consul.Domain;
using BotApp.Extensions.Common.Consul.HostedService;
using BotApp.Extensions.Common.Consul.Services;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace BotApp.Extensions.Common.Consul.Helpers
{
    public class ConsulService : BaseService
    {
        private readonly ConsulConfig config = null;

        public ConsulService(string environmentName, string contentRootPath)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(contentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .AddEnvironmentVariables();

            var configuration = builder.Build();

            config = new ConsulConfig();
            configuration.GetSection("ConsulConfig").Bind(config);

            if (string.IsNullOrEmpty(config.Address))
                throw new ArgumentException("Missing value in ConsulConfig -> Address");

            if (string.IsNullOrEmpty(config.ServiceName))
                throw new ArgumentException("Missing value in ConsulConfig -> ServiceName");

            if (string.IsNullOrEmpty(config.ServiceID))
                throw new ArgumentException("Missing value in ConsulConfig -> ServiceID");

            if (string.IsNullOrEmpty(config.ServiceTag))
                throw new ArgumentException("Missing value in ConsulConfig -> ServiceTag");
        }

        public ConsulConfig GetConfiguration() => config;

        public void Initialize(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHostedService, ConsulHostedService>();
            services.Configure<ConsulConfig>(configuration.GetSection("ConsulConfig"));
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(config.Address);
            }));
        }

        public async Task Stop()
        {
            ConsulClient client = new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(config.Address);
            });

            await client.Agent.ServiceDeregister(ConsulHostedService.RegistrationID);
        }
    }
}