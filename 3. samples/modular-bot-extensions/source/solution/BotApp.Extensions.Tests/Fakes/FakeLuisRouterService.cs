using BotApp.Extensions.BotBuilder.LuisRouter.Domain;
using BotApp.Extensions.BotBuilder.LuisRouter.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp.Extensions.Tests.Fakes
{
    public class FakeLuisRouterService : ILuisRouterService
    {
        private readonly HttpClient httpClient = null;

        public FakeLuisRouterService(HttpClient httpClient = null, UserState userState = null)
        {
            this.httpClient = httpClient;
            this.UserState = userState;
            this.TokenPreference = userState.CreateProperty<string>("TokenPreference");
        }

        public UserState UserState { get; }
        public IStatePropertyAccessor<string> TokenPreference { get; set; }
        public Dictionary<string, LuisRecognizer> LuisServices { get; }

        public LuisRouterConfig GetConfiguration()
        {
            var luisRouterConfig = new LuisRouterConfig()
            {
                BingSpellCheckSubscriptionKey = "bing_spell_check_subscription_key",
                LuisApplications = new List<LuisApp>(),
                LuisRouterUrl = "luis_router_url"
            };
            return luisRouterConfig;
        }

        public async Task GetTokenAsync(WaterfallStepContext step, string encryptedRequest)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { AppIdentity = encryptedRequest }));
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await httpClient.PostAsync($"/identity", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var identityResponse = JsonConvert.DeserializeObject<IdentityResponse>(json);

                    await this.TokenPreference.SetAsync(step.Context, identityResponse.token);
                }
            }
        }

        public async Task<IEnumerable<LuisAppDetail>> LuisDiscoveryAsync(WaterfallStepContext step, string text, string applicationCode, string encryptionKey)
        {
            List<LuisAppDetail> result = new List<LuisAppDetail>();

            int IterationsToRetry = 3;
            int TimeToSleepForRetry = 100;

            for (int i = 0; i <= IterationsToRetry; i++)
            {
                try
                {
                    byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { Text = text, BingSpellCheckSubscriptionKey = "", EnableLuisTelemetry = "" }));
                    using (var content = new ByteArrayContent(byteData))
                    {
                        string token = await this.TokenPreference.GetAsync(step.Context, () => { return string.Empty; });

                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");
                        var response = await httpClient.PostAsync($"/luisdiscovery", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var res = JsonConvert.DeserializeObject<LuisDiscoveryResponse>(json);
                            result = res.Result.LuisAppDetails;
                            break;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            IdentityRequest request = new IdentityRequest()
                            {
                                appcode = applicationCode,
                                timestamp = DateTime.UtcNow
                            };

                            string json = JsonConvert.SerializeObject(request);
                            var encryptedRequest = NETCore.Encrypt.EncryptProvider.AESEncrypt(json, encryptionKey);
                            await GetTokenAsync(step, encryptedRequest);
                            continue;
                        }
                    }
                }
                catch
                {
                    Thread.Sleep(TimeToSleepForRetry);
                    continue;
                }
            }

            return result;
        }
    }
}