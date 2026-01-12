using MongoDB.Driver;

using SteelDesignerEngineer.Data.MongoDB;
using SteelDesignerEngineer.Domain.Entities;

namespace SteelDesignerEngineer.Data.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;
        private readonly PageContentRepository _pageContentRepository;

        public MongoDBService(IMongoDatabase database)
        {
            _database = database;
            _pageContentRepository = new PageContentRepository(_database, "PageContent");
        }
        /// <summary>
        /// Полчить список страниц
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PageContent>> GetAsync()
        {
            return await _pageContentRepository.GetAllAsync();
        }
        /// <summary>
        /// Полчить страницу
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PageContent> GetAsync(Guid id)
        {
            return await _pageContentRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Создать страницу
        /// </summary>
        /// <param name="pageContent"></param>
        /// <returns></returns>
        public async Task CreateAsync(PageContent pageContent)
        {
            await _pageContentRepository.CreateAsync(pageContent);
        }

        
    }
}



#region MyRegion
//public async Task UpdateAsync(Guid id, PageContent pageContent)
//{
//    await _pageContentRepository.UpdateAsync(id, pageContent);
//}

//public async Task RemoveAsync(Guid id)
//{
//    await _pageContentRepository.DeleteAsync(id);
//}
#endregion