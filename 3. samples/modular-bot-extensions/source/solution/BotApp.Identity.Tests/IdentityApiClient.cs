using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Linq;

namespace BotApp.Identity.Tests
{
    public class IdentityApiClient
    {
        private readonly ClientSettings settings;

        public IdentityApiClient(ClientSettings settings)
        {
            this.settings = settings;
        }

        public async Task<AuthenticationResult> GetAccessTokenAsync()
        {
            string[] scopes = { "User.Read" };

            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(settings.ClientId)
                .WithAuthority(new Uri(settings.Authority))
                .Build();

            AuthenticationResult result = null;
            var accounts = await app.GetAccountsAsync();

            try
            {
                result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    result = await app.AcquireTokenWithDeviceCode(scopes, deviceCodeCallback =>
                    {
                        Console.WriteLine(deviceCodeCallback.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();
                }
                catch (MsalException exm)
                {
                    Console.WriteLine($">> Exception: {exm.Message}, StackTrace: {exm.StackTrace}");

                    if (exm.InnerException != null)
                    {
                        Console.WriteLine($">> Inner Exception Message: {exm.InnerException.Message}, Inner Exception StackTrace: {exm.InnerException.StackTrace}");
                    }
                }
            }

            return result;
        }
    }
}