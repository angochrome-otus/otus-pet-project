namespace SteelDesignerEngineer.Domain.Services
{
    /// <summary>
    /// Интерфейс для RabbitMQ подключения (DIP)
    /// </summary>
    public interface IRabbitMqConnection
    {
        Task<bool> IsConnectedAsync();
        void Dispose();
    }
}
