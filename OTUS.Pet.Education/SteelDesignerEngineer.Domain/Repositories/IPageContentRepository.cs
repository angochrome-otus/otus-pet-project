using SteelDesignerEngineer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteelDesignerEngineer.Domain.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с содержимым страниц (DIP)
    /// </summary>
    public interface IPageContentRepository
    {
        Task<PageContent> GetByIdAsync(Guid id);
        Task<PageContent> GetByPageNameAsync(string pageName);
        Task<IEnumerable<PageContent>> GetAllAsync();
        Task<PageContent> CreateAsync(PageContent pageContent);
        Task<PageContent> UpdateAsync(Guid id, PageContent pageContent);
        Task<bool> DeleteAsync(Guid id);
    }
}