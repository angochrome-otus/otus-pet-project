using Microsoft.AspNetCore.Mvc;

namespace OTUS.Pet.Education.Users.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class Users : ControllerBase
{
    private readonly ILogger<Users> _logger;

    public Users(ILogger<Users> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Users")]
    public string GetTest()
    {
        return "It works!";
    }
}
