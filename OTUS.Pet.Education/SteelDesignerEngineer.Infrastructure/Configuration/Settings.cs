namespace SteelDesignerEngineer.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки подключения к базам данных и сервисам
    /// </summary>
    public class ConnectionStringsSettings
    {
        public string MongoDB { get; set; } = string.Empty;
        public string Redis { get; set; } = string.Empty;
        public string RabbitMQ { get; set; } = string.Empty;
    }
    public class DatabaseSettings
    {
        public string DatabaseName { get; set; } = "SteelDesignerEngineer";
    }
    public class CacheSettings
    {
        public int DefaultExpirationMinutes { get; set; }
    }
}
