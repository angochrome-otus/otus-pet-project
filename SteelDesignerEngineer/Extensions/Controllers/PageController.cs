using Microsoft.AspNetCore.Mvc;

namespace SteelDesignerEngineer.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PageController : ControllerBase
    {
        [HttpGet]
        public IActionResult Status()
        {
            return Ok(new { status = "Service running", timestamp = DateTime.UtcNow });
        }
    }
}