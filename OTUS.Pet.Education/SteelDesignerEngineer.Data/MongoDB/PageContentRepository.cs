using MongoDB.Driver;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Data.MongoDB
{
    /// <summary>
    /// MongoDB repository for PageContent
    /// LSP: Properly separated Create, Update, and Upsert operations
    /// </summary>
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

        /// <summary>
        /// Create new page content - throws exception if already exists
        /// LSP: Pure INSERT operation as name promises
        /// </summary>
        public async Task<PageContent> CreateAsync(PageContent pageContent)
        {
            if (pageContent.Id == Guid.Empty)
            {
                pageContent.Id = Guid.NewGuid();
            }
            pageContent.LastModified = DateTime.UtcNow;

            try
            {
                await _collection.InsertOneAsync(pageContent);
                _logger.LogInformation("PageContent created: {PageName} (Id: {Id})", pageContent.PageName, pageContent.Id);
                return pageContent;
            }
            catch (MongoWriteException mwx) when (mwx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogError(mwx, "Duplicate page content: {PageName}", pageContent.PageName);
                throw new InvalidOperationException($"Page content with name '{pageContent.PageName}' already exists.", mwx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PageContent: {PageName}", pageContent.PageName);
                throw;
            }
        }

        /// <summary>
        /// Update existing page content
        /// LSP: Pure UPDATE operation as name promises
        /// </summary>
        public async Task<PageContent> UpdateAsync(Guid id, PageContent pageContent)
        {
            var filter = Builders<PageContent>.Filter.Eq(p => p.Id, id);
            pageContent.Id = id;
            pageContent.LastModified = DateTime.UtcNow;
            
            var result = await _collection.ReplaceOneAsync(filter, pageContent, new ReplaceOptions { IsUpsert = false });
            
            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException($"Page content with ID '{id}' not found.");
            }
            
            _logger.LogInformation("PageContent updated: {PageName} (Id: {Id})", pageContent.PageName, id);
            return pageContent;
        }

        /// <summary>
        /// Create or update page content (upsert operation)
        /// LSP: Explicit UPSERT operation - name clearly indicates behavior
        /// </summary>
        public async Task<PageContent> UpsertAsync(PageContent pageContent)
        {
            if (pageContent.Id == Guid.Empty)
            {
                pageContent.Id = Guid.NewGuid();
            }
            pageContent.LastModified = DateTime.UtcNow;

            try
            {
                var filter = Builders<PageContent>.Filter.Eq(p => p.PageName, pageContent.PageName);
                var options = new ReplaceOptions { IsUpsert = true };

                await _collection.ReplaceOneAsync(filter, pageContent, options);

                _logger.LogInformation("PageContent upserted: {PageName} (Id: {Id})", pageContent.PageName, pageContent.Id);
                return pageContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert PageContent: {PageName}", pageContent.PageName);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var filter = Builders<PageContent>.Filter.Eq(p => p.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}