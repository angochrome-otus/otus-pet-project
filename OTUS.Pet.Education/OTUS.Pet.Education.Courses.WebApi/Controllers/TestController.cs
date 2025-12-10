using Microsoft.AspNetCore.Mvc;
using OTUS.Pet.Education.Courses.Infrastructure.DataLayers.Model;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces.Model;
using OTUS.Pet.Education.Courses.WebApi.Models;

namespace OTUS.Pet.Education.Courses.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    private IUserLayer _userLayer;

    public TestController(ILogger<TestController> logger, IUserLayer userLayer)
    {
        _logger = logger;
        _userLayer = userLayer;
    }

    [HttpGet(Name = "Test")]
    public string GetTest()
    {
        return "It works!";
    }
}
