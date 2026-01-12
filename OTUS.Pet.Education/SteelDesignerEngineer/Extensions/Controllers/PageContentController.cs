using Microsoft.AspNetCore.Mvc;
using SteelDesignerEngineer.Application.Interfaces;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Services;
using SteelDesignerEngineer.Domain.Repositories;
using System.Text.Json;
using System.Linq;

namespace SteelDesignerEngineer.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PageContentController : ControllerBase
    {
        private readonly IPageContentApplicationService _pageContentAppService;
        private readonly ILogger<PageContentController> _logger;
        private readonly IMessageBus _messageBus;
        private readonly ICacheService _cacheService;
        private readonly IPageContentRepository _pageContentRepository;

        public PageContentController(
            IPageContentApplicationService pageContentAppService,
            ILogger<PageContentController> logger,
            IMessageBus messageBus,
            ICacheService cacheService,
            IPageContentRepository pageContentRepository)
        {
            _pageContentAppService = pageContentAppService;
            _logger = logger;
            _messageBus = messageBus;
            _cacheService = cacheService;
            _pageContentRepository = pageContentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _pageContentAppService.GetAllPageContentAsync();
            return Ok(new { success = true, items });
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var items = await _pageContentAppService.GetAllPageContentAsync();
            var count = items?.Count() ?? 0;
            return Ok(new { success = true, count });
        }

        [HttpGet("by-name/{pageName}")]
        public async Task<IActionResult> GetByName(string pageName)
        {
            var item = await _pageContentAppService.GetPageContentByPageNameAsync(pageName);
            if (item == null) return NotFound(new { success = false, error = "NotFound" });
            return Ok(new { success = true, item });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _pageContentAppService.GetPageContentByIdAsync(id);
            if (item == null) return NotFound(new { success = false, error = "NotFound" });
            return Ok(new { success = true, item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PageContent model)
        {
            if (model == null) return BadRequest(new { success = false, error = "Invalid payload" });

            try
            {
                var created = await _pageContentAppService.CreatePageContentAsync(model);

                // Cache the page in Redis
                try
                {
                    var key = $"page:{created.PageName}";
                    var json = JsonSerializer.Serialize(created);
                    await _cacheService.SetAsync(key, json);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cache page in Redis: {PageName}", created.PageName);
                }

                // Publish an event to RabbitMQ about the new page
                try
                {
                    await _messageBus.PublishAsync("page_content_created", created);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to publish page creation event for: {PageName}", created.PageName);
                }

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { success = true, item = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating page content");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpDelete("by-name/{pageName}")]
        public async Task<IActionResult> DeleteByName(string pageName)
        {
            try
            {
                var item = await _pageContentAppService.GetPageContentByPageNameAsync(pageName);
                if (item == null) return NotFound(new { success = false, error = "NotFound" });

                var deleted = await _pageContentRepository.DeleteAsync(item.Id);
                if (!deleted) return NotFound(new { success = false, error = "NotFound" });

                // Remove cache
                try
                {
                    await _cacheService.RemoveDataAsync($"page:{pageName}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to remove cache for page: {PageName}", pageName);
                }

                // Publish deletion event
                try
                {
                    await _messageBus.PublishAsync("page_content_deleted", new { PageName = pageName, Id = item.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to publish page deletion event for: {PageName}", pageName);
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page content: {PageName}", pageName);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
