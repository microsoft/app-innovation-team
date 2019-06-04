using Microsoft.AspNetCore.Mvc;

namespace BotApp.Extensions.Common.Consul.HostedService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult Status() => Ok();
    }
}