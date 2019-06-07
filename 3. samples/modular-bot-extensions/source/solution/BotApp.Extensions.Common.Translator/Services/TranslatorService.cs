using BotApp.Extensions.Common.Translator.Domain;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BotApp.Extensions.Common.Translator.Services
{
    public class TranslatorService : ITranslatorService
    {
        private readonly TranslatorConfig config = null;
        private readonly HttpClient httpClient = null;
        private readonly string translatorUrl = "https://api.cognitive.microsofttranslator.com";

        public TranslatorService(HttpClient httpClient, string environmentName, string contentRootPath)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(contentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .AddEnvironmentVariables();

            var configuration = builder.Build();

            config = new TranslatorConfig();
            configuration.GetSection("TranslatorConfig").Bind(config);

            if (string.IsNullOrEmpty(config.TranslatorTextAPIKey))
                throw new ArgumentException("Missing value in TranslatorConfig -> TranslatorTextAPIKey");

            this.httpClient = httpClient ?? throw new ArgumentException("Missing value in HttpClient");
        }

        public TranslatorConfig GetConfiguration() => config;

        public async Task<string> GetDesiredLanguageAsync(string content)
        {
            string result = string.Empty;
            System.Object[] body = new System.Object[] { new { Text = content } };
            var requestBody = JsonConvert.SerializeObject(body);
            StringContent queryString = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.TranslatorTextAPIKey);
            var response = await httpClient.PostAsync($"{translatorUrl}/detect?api-version=3.0", queryString);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var desiredLanguageResponse = JsonConvert.DeserializeObject<IEnumerable<DesiredLanguageResponse>>(json);
                result = desiredLanguageResponse.ToList()[0].language;
            }
            return result;
        }

        public async Task<string> TranslateSentenceAsync(string content, string originLanguage, string targetLanguage)
        {
            string result = string.Empty;

            if (originLanguage == "en" && targetLanguage == "en")
                return content;

            System.Object[] body = new System.Object[] { new { Text = content } };
            var requestBody = JsonConvert.SerializeObject(body);
            StringContent queryString = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.TranslatorTextAPIKey);
            var response = await httpClient.PostAsync($"{translatorUrl}/translate?api-version=3.0&from={originLanguage}&to={targetLanguage}", queryString);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(json);
                var translations = res[0]["translations"];
                result = translations[0]["text"];
            }
            return result;
        }
    }
}