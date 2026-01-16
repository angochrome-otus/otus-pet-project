using Microsoft.AspNetCore.Mvc;

namespace SteelDesignerEngineer.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/page-status")]
    public class PageStatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult Status()
        {
            return Ok(new { status = "Service running", timestamp = DateTime.UtcNow });
        }
    }
}