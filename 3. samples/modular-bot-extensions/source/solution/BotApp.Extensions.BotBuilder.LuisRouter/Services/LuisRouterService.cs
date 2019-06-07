using BotApp.Extensions.BotBuilder.LuisRouter.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp.Extensions.BotBuilder.LuisRouter.Services
{
    public class LuisRouterService : ILuisRouterService
    {
        private readonly LuisRouterConfig config = null;
        private readonly HttpClient httpClient = null;
        private readonly IBotTelemetryClient botTelemetryClient = null;

        public UserState UserState { get; }
        public IStatePropertyAccessor<string> TokenPreference { get; set; }
        public Dictionary<string, LuisRecognizer> LuisServices { get; }

        public LuisRouterService(HttpClient httpClient, string environmentName, string contentRootPath, UserState userState, IBotTelemetryClient botTelemetryClient = null)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(contentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .AddEnvironmentVariables();

            var configuration = builder.Build();

            config = new LuisRouterConfig();
            configuration.GetSection("LuisRouterConfig").Bind(config);

            this.httpClient = httpClient ?? throw new ArgumentException("Missing value in HttpClient");
            this.botTelemetryClient = botTelemetryClient;
            this.UserState = userState;
            this.LuisServices = BuildDictionary(botTelemetryClient);
            this.TokenPreference = userState.CreateProperty<string>("TokenPreference");
        }

        public LuisRouterConfig GetConfiguration() => config;

        public async Task GetTokenAsync(WaterfallStepContext step, string encryptedRequest)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { AppIdentity = encryptedRequest }));
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await httpClient.PostAsync($"{config.LuisRouterUrl}/identity", content);

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
                    byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { Text = text, BingSpellCheckSubscriptionKey = config.BingSpellCheckSubscriptionKey, EnableLuisTelemetry = (botTelemetryClient == null) ? false : true }));
                    using (var content = new ByteArrayContent(byteData))
                    {
                        string token = await this.TokenPreference.GetAsync(step.Context, () => { return string.Empty; });

                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");
                        var response = await httpClient.PostAsync($"{config.LuisRouterUrl}/luisdiscovery", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var res = JsonConvert.DeserializeObject<LuisDiscoveryResponse>(json);
                            result = res.LuisAppDetails;
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

        private Dictionary<string, LuisRecognizer> BuildDictionary(IBotTelemetryClient botTelemetryClient = null)
        {
            Dictionary<string, LuisRecognizer> result = new Dictionary<string, LuisRecognizer>();

            foreach (LuisApp app in config.LuisApplications)
            {
                var luis = new LuisApplication(app.AppId, app.AuthoringKey, app.Endpoint);

                LuisPredictionOptions luisPredictionOptions = null;
                LuisRecognizer recognizer = null;

                bool needsPredictionOptions = false;
                if ((!string.IsNullOrEmpty(config.BingSpellCheckSubscriptionKey)) || (botTelemetryClient != null))
                {
                    needsPredictionOptions = true;
                }

                if (needsPredictionOptions)
                {
                    luisPredictionOptions = new LuisPredictionOptions();

                    if (botTelemetryClient != null)
                    {
                        luisPredictionOptions.TelemetryClient = botTelemetryClient;
                        luisPredictionOptions.Log = true;
                        luisPredictionOptions.LogPersonalInformation = true;
                    }

                    if (!string.IsNullOrEmpty(config.BingSpellCheckSubscriptionKey))
                    {
                        luisPredictionOptions.BingSpellCheckSubscriptionKey = config.BingSpellCheckSubscriptionKey;
                        luisPredictionOptions.SpellCheck = true;
                        luisPredictionOptions.IncludeAllIntents = true;
                    }

                    recognizer = new LuisRecognizer(luis, luisPredictionOptions);
                }
                else
                {
                    recognizer = new LuisRecognizer(luis);
                }

                result.Add(app.Name, recognizer);
            }

            return result;
        }
    }
}