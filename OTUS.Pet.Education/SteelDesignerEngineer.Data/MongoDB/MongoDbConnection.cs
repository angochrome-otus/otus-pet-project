using MongoDB.Bson;
using MongoDB.Driver;

namespace SteelDesignerEngineer.Data.MongoDB
{
    /// <summary>
    /// Подключение к MongoDB
    /// </summary>
    public class MongoDbConnection : IMongoDbConnection
    {
        private readonly IMongoDatabase _database;

        public MongoDbConnection(IMongoDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}