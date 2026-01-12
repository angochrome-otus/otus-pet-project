namespace SteelDesignerEngineer.Data.MongoDB
{
    public interface IMongoDbConnection
    {
        Task<bool> IsConnectedAsync();
    }
}