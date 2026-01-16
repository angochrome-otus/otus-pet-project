using SteelDesignerEngineer.Domain.Services;
using RabbitMQ.Client;

namespace SteelDesignerEngineer.Infrastructure.Messaging
{
    /// <summary>
    /// Расширенный интерфейс для Infrastructure слоя
    /// Добавляет методы специфичные для RabbitMQ
    /// </summary>
    public interface IRabbitMqConnectionExtended : IRabbitMqConnection
    {
        Task<IConnection> GetConnectionAsync();
    }

    /// <summary>
    /// Реализация подключения к RabbitMQ с DIP
    /// </summary>
    public class RabbitMqConnection : IRabbitMqConnectionExtended, IDisposable
    {
        private readonly Lazy<IConnection> _lazyConnection;
        private bool _disposed;

        public RabbitMqConnection(Lazy<IConnection> lazyConnection)
        {
            _lazyConnection = lazyConnection ?? throw new ArgumentNullException(nameof(lazyConnection));
        }

        public Task<bool> IsConnectedAsync()
        {
            return Task.FromResult(_lazyConnection.IsValueCreated && _lazyConnection.Value.IsOpen);
        }

        public Task<IConnection> GetConnectionAsync()
        {
            return Task.FromResult(_lazyConnection.Value);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_lazyConnection.IsValueCreated)
                {
                    _lazyConnection.Value?.Dispose();
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
