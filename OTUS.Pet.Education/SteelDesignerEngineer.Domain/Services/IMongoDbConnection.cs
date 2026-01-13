namespace SteelDesignerEngineer.Domain.Services
{
    /// <summary>
    /// Interface for MongoDB connection (DIP)
    /// Clean Architecture: Domain interface should be in Domain.Services namespace
    /// </summary>
    public interface IMongoDbConnection
    {
        Task<bool> IsConnectedAsync();
    }
}