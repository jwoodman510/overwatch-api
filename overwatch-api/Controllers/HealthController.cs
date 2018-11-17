using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace overwatch_api.Controllers
{
    [DisableCors]
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new OkObjectResult(new { status = "wearegoodtogo" });
        }
    }
}
