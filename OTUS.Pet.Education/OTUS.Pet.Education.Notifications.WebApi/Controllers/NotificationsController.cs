using Microsoft.AspNetCore.Mvc;

namespace OTUS.Pet.Education.Notifications.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class Notifications : ControllerBase
{
    private readonly ILogger<Notifications> _logger;

    public Notifications(ILogger<Notifications> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Notifications")]
    public string GetTest()
    {
        return "It works!";
    }
}
