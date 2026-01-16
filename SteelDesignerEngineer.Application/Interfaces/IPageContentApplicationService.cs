using SteelDesignerEngineer.Domain.Entities;

namespace SteelDesignerEngineer.Application.Interfaces
{
    public interface IPageContentApplicationService
    {
        /// <summary>
        /// ѕолучить содержание всейх страниц все страницы
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PageContent>> GetAllPageContentAsync();
        /// <summary>
        /// ѕолучить содержание одной страницы
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<PageContent> GetPageContentByIdAsync(Guid id);
        /// <summary>
        /// ѕолучить содержание одной страницы по имени страницы
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        Task<PageContent> GetPageContentByPageNameAsync(string pageName);
        /// <summary>
        /// создать страницу с содержимым... ???
        /// </summary>
        /// <param name="pageContent"></param>
        /// <returns></returns>
        Task<PageContent> CreatePageContentAsync(PageContent pageContent);
        Task UpdatePageContentAsync(Guid id, PageContent pageContent);
        Task DeletePageContentAsync(Guid id);
    }
}

