using Microsoft.AspNetCore.Mvc;

namespace RF.Contracts.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult Status() => Ok();
    }
}