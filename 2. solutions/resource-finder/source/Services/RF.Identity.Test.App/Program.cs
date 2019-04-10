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
            var todoApiClient = new IdentityApiClient(settings);
            await todoApiClient.UserRegistrationAsync();
            Console.WriteLine("Press Enter to quit");
            Console.ReadLine();
        }

        private static IConfiguration CreateConfig() =>
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();
    }
}