using Microsoft.AspNetCore.Mvc;

namespace OTUS.Pet.Education.Orders.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class Orders : ControllerBase
{
    private readonly ILogger<Orders> _logger;

    public Orders(ILogger<Orders> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Orders")]
    public string GetTest()
    {
        return "It works!";
    }
}
