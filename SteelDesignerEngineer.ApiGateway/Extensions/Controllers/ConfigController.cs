using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SteelDesignerEngineer.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("site-settings")]
        public IActionResult GetSiteSettings()
        {
            var section = _configuration.GetSection("SiteSettings");
            var defaultPage = section.GetValue<string>("DefaultPage", "home");
            var enableCache = section.GetValue<bool>("EnableCache", true);
            var cacheDuration = section.GetValue<int>("CacheDurationMinutes", 60);

            return Ok(new
            {
                success = true,
                settings = new
                {
                    DefaultPage = defaultPage,
                    EnableCache = enableCache,
                    CacheDurationMinutes = cacheDuration
                }
            });
        }
    }
}
