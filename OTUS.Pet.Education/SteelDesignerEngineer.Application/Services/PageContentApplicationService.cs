using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Application.Interfaces;
using SteelDesignerEngineer.Domain.Services;

namespace SteelDesignerEngineer.Application.Services
{
    public class PageContentApplicationService : IPageContentApplicationService
    {

        private readonly IPageContentService _pageContentService;


        /// <summary>
        /// Создание экземпляра
        /// </summary>
        /// <param name="pageContentService"></param>
        /// <param name="messageProducer"></param>
        public PageContentApplicationService(IPageContentService pageContentService)
        {
            _pageContentService = pageContentService;
        }
        
        /// <summary>
        /// Отправляем сообщение в RabbitMQ о запросе данных
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PageContent>> GetAllPageContentAsync()
        {
            // Отправляем сообщение в RabbitMQ о запросе данных
            
            return await _pageContentService.GetAllPageContentAsync();
        }

        /// <summary>
        /// Отправляем сообщение в RabbitMQ о запросе данных для конкретной страницы
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        public async Task<PageContent> GetPageContentByPageNameAsync(string pageName)
        {
            // Отправляем сообщение в RabbitMQ о запросе данных для конкретной страницы
            
            return await _pageContentService.GetPageContentByPageNameAsync(pageName);
        }

        /// <summary>
        /// Отправляем сообщение в RabbitMQ о запросе данных для конкретной страницы по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PageContent> GetPageContentByIdAsync(Guid id)
        {
            // Отправляем сообщение в RabbitMQ о запросе данных для конкретной страницы по ID
            
            return await _pageContentService.GetPageContentByIdAsync(id);
        }
        /// <summary>
        /// Отправляем сообщение в RabbitMQ о создании новой страницы
        /// </summary>
        /// <param name="pageContent"></param>
        /// <returns></returns>
        public async Task<PageContent> CreatePageContentAsync(PageContent pageContent)
        {
            var created = await _pageContentService.CreatePageContentAsync(pageContent);

            Console.WriteLine("Отправляем сообщение в RabbitMQ о создании новой страницы");
            
            return created;
        }

        public async Task UpdatePageContentAsync(Guid id, PageContent pageContent)
        {
            await _pageContentService.UpdatePageContentAsync(id, pageContent);
            Console.WriteLine("Отправляем сообщение в RabbitMQ об обновлении страницы");
        }

        public async Task DeletePageContentAsync(Guid id)
        {
            await _pageContentService.DeletePageContentAsync(id);
            Console.WriteLine($"Удалена страница с ID: {id}");
        }
    }
}