using Microsoft.AspNetCore.Mvc;

namespace BotApp.Luis.Router.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult Status() => Ok();
    }
}