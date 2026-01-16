using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса для работы с содержимым страниц
    /// Clean Architecture: Domain Service implementation in Infrastructure layer
    /// </summary>
    public class PageContentService : IPageContentService
    {
        private readonly IPageContentRepository _pageContentRepository;
        private readonly ILogger<PageContentService> _logger;

        public PageContentService(IPageContentRepository pageContentRepository, ILogger<PageContentService> logger)
        {
            _pageContentRepository = pageContentRepository;
            _logger = logger;
        }

        public async Task<PageContent> GetPageContentByIdAsync(Guid id)
        {
            return await _pageContentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PageContent>> GetAllPageContentAsync()
        {
            return await _pageContentRepository.GetAllAsync();
        }

        public async Task<PageContent> GetPageContentByPageNameAsync(string pageName)
        {
            return await _pageContentRepository.GetByPageNameAsync(pageName);
        }

        public async Task<PageContent> CreatePageContentAsync(PageContent pageContent)
        {
            try
            {
                var created = await _pageContentRepository.CreateAsync(pageContent);
                _logger.LogInformation("PageContent created: {PageName} (Id: {Id})", created.PageName, created.Id);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating PageContent: {PageName}", pageContent?.PageName);
                throw;
            }
        }

        public async Task UpdatePageContentAsync(Guid id, PageContent pageContent)
        {
            await _pageContentRepository.UpdateAsync(id, pageContent);
        }

        public async Task DeletePageContentAsync(Guid id)
        {
            await _pageContentRepository.DeleteAsync(id);
        }
    }
}