using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotApp.Extensions.BotBuilder.Channel.WebChat.Services
{
    public class WebChatService : IWebChatService
    {
        private readonly WebChatConfig config = null;
        private readonly HttpClient httpClient = null;

        public WebChatService(HttpClient httpClient, string environmentName, string contentRootPath)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(contentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .AddEnvironmentVariables();

            var configuration = builder.Build();

            config = new WebChatConfig();
            configuration.GetSection("WebChatConfig").Bind(config);

            if (string.IsNullOrEmpty(config.Secret))
                throw new ArgumentException("Missing value in WebChatConfig -> Secret");

            this.httpClient = httpClient ?? throw new ArgumentException("Missing value in HttpClient");
        }

        public WebChatConfig GetConfiguration() => config;

        public async Task<GenerateResponse> GetDirectLineTokenAsync(string secret)
        {
            GenerateResponse result = null;
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }));
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{secret}");
                var response = await httpClient.PostAsync($"https://directline.botframework.com/v3/directline/tokens/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<GenerateResponse>(json);
                }
            }
            return result;
        }
    }
}