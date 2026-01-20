using Microsoft.AspNetCore.Mvc;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Controllers;

[ApiController]
[Route("api/page-contents")]
public class PageContentAdminController : ControllerBase
{
    private readonly IPageContentServiceClient _pageContent;

    public PageContentAdminController(IPageContentServiceClient pageContent)
    {
        _pageContent = pageContent;
    }

    private bool IsAdmin()
    {
        if (!HttpContext.IsSessionAuthenticated())
        {
            return false;
        }

        var role = (HttpContext.Items.TryGetValue("UserRole", out var r) ? r as string : null) ?? string.Empty;
        return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (!IsAdmin()) return Forbid();

        var res = await _pageContent.GetAllPagesAsync(new GetAllPagesRequest(), cancellationToken);
        if (res == null) return StatusCode(503, new { message = "PageContent service unavailable" });

        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePageRequest request, CancellationToken cancellationToken)
    {
        if (!IsAdmin()) return Forbid();

        var res = await _pageContent.CreatePageAsync(request, cancellationToken);
        if (res == null) return StatusCode(503, new { message = "PageContent service unavailable" });

        return Ok(res);
    }

    [HttpPut("{pageId}")]
    public async Task<IActionResult> Update(string pageId, [FromBody] UpdatePageRequest request, CancellationToken cancellationToken)
    {
        if (!IsAdmin()) return Forbid();

        var req = request with { PageId = pageId };
        var res = await _pageContent.UpdatePageAsync(req, cancellationToken);
        if (res == null) return StatusCode(503, new { message = "PageContent service unavailable" });

        return Ok(res);
    }

    [HttpDelete("by-name/{pageName}")]
    public async Task<IActionResult> DeleteByName(string pageName, CancellationToken cancellationToken)
    {
        if (!IsAdmin()) return Forbid();

        var res = await _pageContent.DeletePageAsync(new DeletePageRequest { PageName = pageName }, cancellationToken);
        if (res == null) return StatusCode(503, new { message = "PageContent service unavailable" });

        return Ok(res);
    }
}
