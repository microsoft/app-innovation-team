using Microsoft.AspNetCore.Mvc;

namespace BotApp
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult Status() => Ok();
    }
}