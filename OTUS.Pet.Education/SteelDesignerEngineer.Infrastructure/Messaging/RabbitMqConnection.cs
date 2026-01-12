using SteelDesignerEngineer.Domain.Services;
using RabbitMQ.Client;

namespace SteelDesignerEngineer.Infrastructure.Messaging
{
    /// <summary>
    /// Реализация подключения к RabbitMQ с DIP
    /// </summary>
    public class RabbitMqConnection : IRabbitMqConnection, IDisposable
    {
        private readonly IConnection _connection;
        private bool _disposed;

        public RabbitMqConnection(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public Task<bool> IsConnectedAsync()
        {
            return Task.FromResult(_connection.IsOpen);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
