using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace RF.Identity.Test.App
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IConfiguration config = CreateConfig();
            ClientSettings settings = config.Get<ClientSettings>();
            var identityApiClient = new IdentityApiClient(settings);

            while(true)
            {
                //await identityApiClient.UserRegistrationAsync();
                await identityApiClient.ContractDeploymentnAsync();
                Console.WriteLine("Press Enter to execute it again or close the window if you want to exit");
                Console.ReadLine();
            }
        }

        private static IConfiguration CreateConfig() =>
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();
    }
}