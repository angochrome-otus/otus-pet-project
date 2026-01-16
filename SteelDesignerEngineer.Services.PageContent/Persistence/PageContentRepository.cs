using MongoDB.Driver;

namespace SteelDesignerEngineer.Services.PageContent.Persistence;

internal sealed class PageContentRepository
{
    private readonly IMongoCollection<PageContentDocument> _collection;

    public PageContentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PageContentDocument>("pageContents");

        var pageNameIndex = new CreateIndexModel<PageContentDocument>(
            Builders<PageContentDocument>.IndexKeys.Ascending(x => x.PageName),
            new CreateIndexOptions { Unique = true });

        _collection.Indexes.CreateOne(pageNameIndex);
    }

    public Task<PageContentDocument?> GetByIdAsync(string id, CancellationToken ct = default)
        => _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public Task<PageContentDocument?> GetByPageNameAsync(string pageName, CancellationToken ct = default)
        => _collection.Find(x => x.PageName == pageName).FirstOrDefaultAsync(ct);

    public Task<List<PageContentDocument>> GetAllAsync(CancellationToken ct = default)
        => _collection.Find(_ => true).ToListAsync(ct);

    public async Task<PageContentDocument> UpsertAsync(PageContentDocument doc, CancellationToken ct = default)
    {
        doc.LastModified = DateTime.UtcNow;

        var filter = Builders<PageContentDocument>.Filter.Eq(x => x.PageName, doc.PageName);
        await _collection.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true }, ct);
        return doc;
    }

    public Task<bool> DeleteByPageNameAsync(string pageName, CancellationToken ct = default)
        => _collection.DeleteOneAsync(x => x.PageName == pageName, ct).ContinueWith(t => t.Result.DeletedCount > 0, ct);
}

internal sealed class PageContentDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PageName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
