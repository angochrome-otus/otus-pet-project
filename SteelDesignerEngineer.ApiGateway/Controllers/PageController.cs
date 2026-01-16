using Microsoft.AspNetCore.Mvc;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Controllers;

public class PageController : Controller
{
    private readonly IPageContentServiceClient _pageContentService;
    private readonly ILogger<PageController> _logger;

    public PageController(
        IPageContentServiceClient pageContentService,
        ILogger<PageController> logger)
    {
        _pageContentService = pageContentService;
        _logger = logger;
    }

    [HttpGet("/{pageName}")]
    public async Task<IActionResult> Show(string pageName)
    {
        try
        {
            var response = await _pageContentService.GetPageByNameAsync(new GetPageByNameRequest { PageName = pageName });

            if (response == null || !response.Success)
            {
                _logger.LogWarning("Page not found: {PageName}", pageName);
                return NotFound();
            }

            ViewBag.Title = response.Title;
            ViewBag.MetaDescription = response.MetaDescription;
            ViewBag.MetaKeywords = response.MetaKeywords;
            ViewBag.Content = response.Content;

            return View("DynamicPage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading page: {PageName}", pageName);
            return StatusCode(500, "Error loading page");
        }
    }
}
