using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace SteelDesignerEngineer.Services.PageContent.Persistence;

internal sealed class PageContentRepository
{
    private readonly IMongoCollection<PageContentDocument> _collection;

    public PageContentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PageContentDocument>("pageContents");
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

        var filter = !string.IsNullOrWhiteSpace(doc.Id)
            ? Builders<PageContentDocument>.Filter.Eq(x => x.Id, doc.Id)
            : Builders<PageContentDocument>.Filter.Eq(x => x.PageName, doc.PageName);

        await _collection.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true }, ct);
        return doc;
    }

    public async Task<bool> DeleteByPageNameAsync(string pageName, CancellationToken ct = default)
    {
        var res = await _collection.DeleteOneAsync(x => x.PageName == pageName, ct);
        return res.DeletedCount > 0;
    }
}

internal sealed class PageContentDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string PageName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
