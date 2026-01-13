using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Application.Interfaces;
using SteelDesignerEngineer.Domain.Services;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Application.Services
{
    /// <summary>
    /// Application service for PageContent operations
    /// SRP: Coordinates domain services and publishes events
    /// </summary>
    public class PageContentApplicationService : IPageContentApplicationService
    {
        private readonly IPageContentService _pageContentService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<PageContentApplicationService> _logger;

        public PageContentApplicationService(
            IPageContentService pageContentService,
            IMessagePublisher messagePublisher,
            ILogger<PageContentApplicationService> logger)
        {
            _pageContentService = pageContentService;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }
        
        public async Task<IEnumerable<PageContent>> GetAllPageContentAsync()
        {
            return await _pageContentService.GetAllPageContentAsync();
        }

        public async Task<PageContent> GetPageContentByPageNameAsync(string pageName)
        {
            return await _pageContentService.GetPageContentByPageNameAsync(pageName);
        }

        public async Task<PageContent> GetPageContentByIdAsync(Guid id)
        {
            return await _pageContentService.GetPageContentByIdAsync(id);
        }

        public async Task<PageContent> CreatePageContentAsync(PageContent pageContent)
        {
            var created = await _pageContentService.CreatePageContentAsync(pageContent);

            // Publish event to message bus
            try
            {
                await _messagePublisher.PublishAsync("page_content_created", new
                {
                    created.Id,
                    created.PageName,
                    created.Title,
                    Timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation("PageContent created and event published: {PageName} (Id: {Id})", 
                    created.PageName, created.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish page_content_created event for {PageName}", created.PageName);
                // Don't fail the operation if event publishing fails
            }
            
            return created;
        }

        public async Task UpdatePageContentAsync(Guid id, PageContent pageContent)
        {
            await _pageContentService.UpdatePageContentAsync(id, pageContent);

            // Publish update event
            try
            {
                await _messagePublisher.PublishAsync("page_content_updated", new
                {
                    id,
                    pageContent.PageName,
                    pageContent.Title,
                    Timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation("PageContent updated and event published: {PageName} (Id: {Id})", 
                    pageContent.PageName, id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish page_content_updated event for {PageName}", pageContent.PageName);
            }
        }

        public async Task DeletePageContentAsync(Guid id)
        {
            await _pageContentService.DeletePageContentAsync(id);

            // Publish delete event
            try
            {
                await _messagePublisher.PublishAsync("page_content_deleted", new
                {
                    Id = id,
                    Timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation("PageContent deleted and event published: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish page_content_deleted event for {Id}", id);
            }
        }
    }
}