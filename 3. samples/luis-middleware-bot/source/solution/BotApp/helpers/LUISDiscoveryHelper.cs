using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class LUISDiscoveryHelper
    {
        private static string Token { get; set; } = string.Empty;

        public async static Task GetTokenAsync(string applicationCode)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { ApplicationCode = applicationCode }));
                    using (var content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await client.PostAsync($"{Settings.LuisMiddlewareUrl}/appauthentication", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var token = JsonConvert.DeserializeObject<Token>(json);
                            Token = token.token;
                        }
                    }
                }
            }
        }

        public async static Task<List<LuisAppDetail>> LuisDiscoveryAsync(string text)
        {
            List<LuisAppDetail> result = new List<LuisAppDetail>();

            int IterationsToRetry = 3;
            int TimeToSleepForRetry = 100;

            for (int i = 0; i <= IterationsToRetry; i++)
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (HttpClient client = new HttpClient(handler))
                    {
                        try
                        {
                            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { Text = text }));
                            using (var content = new ByteArrayContent(byteData))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");
                                var response = await client.PostAsync($"{Settings.LuisMiddlewareUrl}/luisdiscovery", content);

                                if (response.IsSuccessStatusCode)
                                {
                                    var json = await response.Content.ReadAsStringAsync();
                                    var res = JsonConvert.DeserializeObject<LuisDiscoveryResponseResult>(json);
                                    result = res.Result.LuisAppDetails;
                                    break;
                                }
                                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    await GetTokenAsync(Startup.EncryptedKey);
                                    continue;
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            Thread.Sleep(TimeToSleepForRetry);
                            continue;
                        }
                    }
                }
            }

            return result;
        }
    }
}