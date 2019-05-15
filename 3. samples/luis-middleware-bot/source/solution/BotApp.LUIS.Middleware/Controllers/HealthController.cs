using Microsoft.AspNetCore.Mvc;

namespace BotApp.LUIS.Middleware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult Status() => Ok();
    }
}