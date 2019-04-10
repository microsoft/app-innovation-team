using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RF.Identity.Test.App
{
    public class IdentityApiClient
    {
        private readonly ClientSettings _settings;

        public IdentityApiClient(ClientSettings settings)
        {
            _settings = settings;
        }

        public async Task UserRegistrationAsync()
        {
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (HttpClient client = new HttpClient(handler))
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Post, $"{_settings.ApiBaseUrl}/userregistration"))
                    {
                        string accessToken = await GetAccessTokenAsync();
                        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                        using (HttpResponseMessage res = await client.SendAsync(req))
                        {
                            res.EnsureSuccessStatusCode();
                            string json = await res.Content.ReadAsStringAsync();
                            Console.WriteLine($"Json result: {json}");
                        }
                    }
                }
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var context = new AuthenticationContext(_settings.Authority);

            AuthenticationResult result;
            try
            {
                result = await context.AcquireTokenSilentAsync(_settings.ApiResourceUri, _settings.ClientId);
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                DeviceCodeResult deviceCodeResult = await context.AcquireDeviceCodeAsync(_settings.ApiResourceUri, _settings.ClientId);
                Console.WriteLine(deviceCodeResult.Message);
                result = await context.AcquireTokenByDeviceCodeAsync(deviceCodeResult);
            }

            return result.AccessToken;
        }
    }
}