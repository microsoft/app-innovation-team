using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RF.Identity.Domain.Entities.ServiceDiscovery;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RF.Identity.Api.HostedService.ServiceDiscovery
{
    public class ConsulHostedService : IHostedService
    {
        private CancellationTokenSource _cts = null;
        private readonly IConsulClient _consulClient = null;
        private readonly IOptions<ConsulConfig> _consulConfig = null;
        private readonly ILogger<ConsulHostedService> _logger = null;
        private readonly IServer _server = null;
        public static string RegistrationID { get; set; }

        public ConsulHostedService(IConsulClient consulClient, IOptions<ConsulConfig> consulConfig, ILogger<ConsulHostedService> logger, IServer server)
        {
            _server = server;
            _logger = logger;
            _consulConfig = consulConfig;
            _consulClient = consulClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var features = _server.Features;
            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.First();

            var uri = new Uri(address);
            var name = Dns.GetHostName(); // to get the container id
            var ip = Dns.GetHostEntry(name).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

            var registration = new AgentServiceRegistration()
            {
                ID = $"{ip}",
                Name = $"{_consulConfig.Value.ServiceName}",
                Address = $"{ip}",
                Port = uri.Port,
                Tags = new[] { $"{_consulConfig.Value.ServiceTag}" },
                Check = new AgentServiceCheck()
                {
                    HTTP = $"{uri.Scheme}://{ip}:{uri.Port}/api/health/status",
                    Timeout = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromSeconds(10),
                    TLSSkipVerify = true
                }
            };

            RegistrationID = registration.ID;
            _logger.LogInformation($"{uri.Scheme}://{ip}:{uri.Port}/api/health/status");
            _logger.LogInformation("Registering in Consul");
            await _consulClient.Agent.ServiceDeregister(registration.ID, _cts.Token);
            await _consulClient.Agent.ServiceRegister(registration, _cts.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _logger.LogInformation("Deregistering from Consul");
            try
            {
                await _consulClient.Agent.ServiceDeregister(RegistrationID, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Deregisteration failed");
            }
        }
    }
}