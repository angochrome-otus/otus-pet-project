using MongoDB.Driver;
using SteelDesignerEngineer.Domain.Entities;

namespace SteelDesignerEngineer.Data.MongoDB
{
    /// <summary>
    /// Контекст базы данных MongoDB
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<PageContent> PageContentCollection
        {
            get
            {
                return _database.GetCollection<PageContent>("PageContent");
            }
        }
    }
}