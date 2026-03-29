using Microsoft.AspNetCore.Mvc;

namespace MicrobloggingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "PlayerPulse API is running!",
                version = "1.0.0",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
