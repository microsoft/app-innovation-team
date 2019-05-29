using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;
using BotApp.Extensions.BotBuilder.Channel.WebChat.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotApp.Extensions.BotBuilder.Channel.WebChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectLineTokenController : ControllerBase
    {
        private readonly IWebChatService webChatService;

        public DirectLineTokenController(IWebChatService webChatService)
        {
            this.webChatService = webChatService;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            GenerateResult result = null;

            using (HttpClient client = new HttpClient())
            {
                byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }));
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{webChatService.GetConfiguration().Secret}");
                    var response = await client.PostAsync($"https://directline.botframework.com/v3/directline/tokens/generate", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<GenerateResult>(json);
                    }
                }
            }

            return (ActionResult)new OkObjectResult(new { result.token });
        }
    }
}