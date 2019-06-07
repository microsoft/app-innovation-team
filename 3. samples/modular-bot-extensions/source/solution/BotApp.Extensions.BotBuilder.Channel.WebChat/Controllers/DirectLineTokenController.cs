using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;
using BotApp.Extensions.BotBuilder.Channel.WebChat.Services;
using Microsoft.AspNetCore.Mvc;
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
            GenerateResponse result = await webChatService.GetDirectLineTokenAsync(webChatService.GetConfiguration().Secret);
            return (ActionResult)new OkObjectResult(new { result.token });
        }
    }
}