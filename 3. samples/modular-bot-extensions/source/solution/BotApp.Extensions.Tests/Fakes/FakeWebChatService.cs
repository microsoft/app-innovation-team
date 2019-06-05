using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;
using BotApp.Extensions.BotBuilder.Channel.WebChat.Services;
using Newtonsoft.Json;

namespace BotApp.Extensions.Tests.Fakes
{
    public class FakeWebChatService : IWebChatService
    {
        private readonly HttpClient httpClient = null;

        public FakeWebChatService(HttpClient httpClient = null)
        {
            this.httpClient = httpClient;
        }

        public WebChatConfig GetConfiguration()
        {
            var webChatConfig = new WebChatConfig()
            {
                Secret = "secret"
            };
            return webChatConfig;
        }

        public async Task<GenerateResult> GetDirectLineTokenAsync(string secret)
        {
            GenerateResult result = null;
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }));
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{secret}");
                var response = await httpClient.PostAsync($"https://directline.botframework.com/v3/directline/tokens/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<GenerateResult>(json);
                }
            }
            return result;
        }
    }
}