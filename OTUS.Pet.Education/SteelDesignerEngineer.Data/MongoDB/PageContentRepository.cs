using MongoDB.Driver;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Data.MongoDB
{
    public class PageContentRepository : IPageContentRepository
    {
        private readonly IMongoCollection<PageContent> _collection;
        private readonly ILogger<PageContentRepository> _logger;

        public PageContentRepository(IMongoDatabase database, string collectionName = "PageContents", ILogger<PageContentRepository>? logger = null)
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PageContentRepository>.Instance;
            _collection = database.GetCollection<PageContent>(collectionName);
            try
            {
                var dbName = database.DatabaseNamespace?.DatabaseName ?? "<unknown>";
                _logger.LogInformation("PageContentRepository initialized. Database: {Database}, Collection: {Collection}", dbName, collectionName);
            }
            catch
            {
                // ignore logging errors
            }
        }

        public async Task<IEnumerable<PageContent>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<PageContent> GetByIdAsync(Guid id)
        {
            var filter = Builders<PageContent>.Filter.Eq(p => p.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<PageContent> GetByPageNameAsync(string pageName)
        {
            var filter = Builders<PageContent>.Filter.Eq(p => p.PageName, pageName);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<PageContent> CreateAsync(PageContent pageContent)
        {
            if (pageContent.Id == Guid.Empty)
            {
                pageContent.Id = Guid.NewGuid();
            }
            pageContent.LastModified = DateTime.UtcNow;

            try
            {
                // Use upsert by PageName to avoid duplicate _id errors and ensure a single document per page
                var filter = Builders<PageContent>.Filter.Eq(p => p.PageName, pageContent.PageName);
                var options = new ReplaceOptions { IsUpsert = true };

                await _collection.ReplaceOneAsync(filter, pageContent, options);

                _logger.LogInformation("Upserted PageContent: {PageName} (Id: {Id})", pageContent.PageName, pageContent.Id);
                return pageContent;
            }
            catch (MongoWriteException mwx) when (mwx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogWarning(mwx, "Duplicate key on insert for PageContent {PageName}, attempting to fetch existing document", pageContent.PageName);

                // Fetch existing document and return it
                var existing = await GetByPageNameAsync(pageContent.PageName);
                if (existing != null)
                {
                    return existing;
                }

                // If not found, rethrow
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert/upsert PageContent: {PageName}", pageContent.PageName);
                throw;
            }
        }

        public async Task<PageContent> UpdateAsync(Guid id, PageContent pageContent)
        {
            var filter = Builders<PageContent>.Filter.Eq(p => p.Id, id);
            pageContent.Id = id;
            pageContent.LastModified = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(filter, pageContent, new ReplaceOptions { IsUpsert = false });
            return pageContent;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var filter = Builders<PageContent>.Filter.Eq(p => p.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}