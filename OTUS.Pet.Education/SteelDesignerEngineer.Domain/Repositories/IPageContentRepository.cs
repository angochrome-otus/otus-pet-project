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
        
        /// <summary>
        /// Создание нового содержимого страницы (необходимо, чтобы страница еще не существовала)
        /// </summary>
        Task<PageContent> CreateAsync(PageContent pageContent);
        
        /// <summary>
        /// Обновление существующего содержимого страницы
        /// </summary>
        Task<PageContent> UpdateAsync(Guid id, PageContent pageContent);
        
        /// <summary>
        /// Создание или обновление содержимого страницы (операция upsert)
        /// </summary>
        Task<PageContent> UpsertAsync(PageContent pageContent);
        
        Task<bool> DeleteAsync(Guid id);
    }
}