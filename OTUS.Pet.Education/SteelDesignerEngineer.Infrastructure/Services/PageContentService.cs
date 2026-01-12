using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SteelDesignerEngineer.Data.MongoDB;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса для работы с содержимым страниц
    /// </summary>
    public class PageContentService : IPageContentService
    {
        /// <summary>
        /// Договор на создание страеницы
        /// </summary>
        private readonly IPageContentRepository _pageContentRepository;
        private readonly ILogger<PageContentService> _logger;

        /// <summary>
        /// Экземпляр страницы 
        /// </summary>
        /// <param name="pageContentRepository"></param>
        public PageContentService(IPageContentRepository pageContentRepository, ILogger<PageContentService> logger)
        {
            _pageContentRepository = pageContentRepository;
            _logger = logger;
        }

        /// <summary>
        /// Получить страницу по ID guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PageContent> GetPageContentByIdAsync(Guid id)
        {
            return await _pageContentRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Получить все страницы по ID guid
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PageContent>> GetAllPageContentAsync()
        {
            return await _pageContentRepository.GetAllAsync();
        }

        /// <summary>
        /// Получить страницу по имени
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        public async Task<PageContent> GetPageContentByPageNameAsync(string pageName)
        {
            return await _pageContentRepository.GetByPageNameAsync(pageName);
        }

        /// <summary>
        /// Создать страницу
        /// </summary>
        /// <param name="pageContent"></param>
        /// <returns></returns>
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