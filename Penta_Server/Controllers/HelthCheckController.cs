using Microsoft.AspNetCore.Mvc;

namespace Penta_Server.Controllers
{
    [Route("WorkStatus")]
    [ApiController]
    public class HelthCheckController : ControllerBase
    {
        [HttpGet("Check")]
        public IActionResult AmAlive()
        {
            return Ok();
        }
    }
}
