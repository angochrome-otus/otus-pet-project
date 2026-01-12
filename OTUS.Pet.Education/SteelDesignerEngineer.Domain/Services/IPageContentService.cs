using SteelDesignerEngineer.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteelDesignerEngineer.Domain.Services
{
    /// <summary>
    /// Интерфейс сервиса для работы с содержимым страниц (DIP)
    /// </summary>
    public interface IPageContentService
    {
        Task<IEnumerable<PageContent>> GetAllPageContentAsync();
        Task<PageContent> GetPageContentByPageNameAsync(string pageName);
        Task<PageContent> GetPageContentByIdAsync(Guid id);
        Task<PageContent> CreatePageContentAsync(PageContent pageContent);
        Task UpdatePageContentAsync(Guid id, PageContent pageContent);
        Task DeletePageContentAsync(Guid id);
    }
}