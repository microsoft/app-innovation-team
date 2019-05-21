using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

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
            var context = new AuthenticationContext(settings.Authority);

            AuthenticationResult result;
            try
            {
                result = await context.AcquireTokenSilentAsync(settings.ApiResourceUri, settings.ClientId);
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                DeviceCodeResult deviceCodeResult = await context.AcquireDeviceCodeAsync(settings.ApiResourceUri, settings.ClientId);
                Console.WriteLine(deviceCodeResult.Message);
                result = await context.AcquireTokenByDeviceCodeAsync(deviceCodeResult);
            }

            return result;
        }
    }
}