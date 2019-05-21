using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace BotApp.Identity.Tests
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IConfiguration config = CreateConfig();
            ClientSettings settings = config.Get<ClientSettings>();

            IdentityApiClient identityApiClient = new IdentityApiClient(settings);
            AuthenticationResult result = await identityApiClient.GetAccessTokenAsync();

            Console.WriteLine($"AccessToken: {result.AccessToken}");
            Console.WriteLine($"AccessTokenType: {result.AccessTokenType}");
            Console.WriteLine($"Authority: {result.Authority}");
            Console.WriteLine($"ExpiresOn: {result.ExpiresOn}");
            Console.WriteLine($"ExtendedLifeTimeToken: {result.ExtendedLifeTimeToken}");
            Console.WriteLine($"IdToken: {result.IdToken}");
            Console.WriteLine($"TenantId: {result.TenantId}");
            Console.WriteLine($"Fullname: {result.UserInfo.GivenName} {result.UserInfo.FamilyName}");

            Console.WriteLine("Press Enter to execute it again or close the window if you want to exit");
            Console.ReadLine();
        }

        private static IConfiguration CreateConfig() =>
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();
    }
}