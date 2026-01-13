using SteelDesignerEngineer.Domain.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Infrastructure.Messaging
{
    /// <summary>
    /// Message serializer interface - OCP compliance
    /// </summary>
    public interface IMessageSerializer
    {
        string Serialize<T>(T message);
        T? Deserialize<T>(string data);
    }

    /// <summary>
    /// Default JSON serializer implementation
    /// </summary>
    public class JsonMessageSerializer : IMessageSerializer
    {
        public string Serialize<T>(T message)
        {
            return JsonSerializer.Serialize(message);
        }

        public T? Deserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }
    }

    /// <summary>
    /// Реализация шины сообщений с использованием RabbitMQ с DIP
    /// Lazy Loading - подключение создается только при первом использовании
    /// SOLID compliant: SRP, OCP, LSP, ISP, DIP
    /// </summary>
    public class RabbitMqMessageBus : IMessageBus, IDisposable
    {
        private readonly IRabbitMqConnection _rabbitMqConnection;
        private readonly Lazy<IChannel> _channel;
        private readonly IMessageSerializer _serializer;
        private readonly ILogger<RabbitMqMessageBus>? _logger;
        private bool _disposed;

        public RabbitMqMessageBus(
            Lazy<IConnection> lazyConnection, 
            IRabbitMqConnection rabbitMqConnection,
            IMessageSerializer? serializer = null,
            ILogger<RabbitMqMessageBus>? logger = null)
        {
            _rabbitMqConnection = rabbitMqConnection ?? throw new ArgumentNullException(nameof(rabbitMqConnection));
            _serializer = serializer ?? new JsonMessageSerializer(); // Default serializer
            _logger = logger;
            
            // Lazy initialization of channel
            _channel = new Lazy<IChannel>(() =>
            {
                _logger?.LogInformation("Creating RabbitMQ channel (Lazy Loading)");
                var connection = lazyConnection.Value; // Connection created here on first access
                return connection.CreateChannelAsync().GetAwaiter().GetResult();
            });
        }

        public async Task PublishAsync<T>(string queueName, T message)
        {
            try
            {
                // Декларация очереди (создается, если не существует)
                await _channel.Value.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // ? OCP: Используем абстракцию IMessageSerializer
                var json = _serializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                // Отправка с подтверждением для персистентности
                var properties = new BasicProperties
                {
                    Persistent = true
                };

                await _channel.Value.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body
                );

                _logger?.LogDebug("Message published to queue: {QueueName}", queueName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error publishing message to queue: {QueueName}", queueName);
                throw;
            }
        }

        public async Task<T> ConsumeAsync<T>(string queueName, TimeSpan timeout)
        {
            try
            {
                await _channel.Value.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                BasicGetResult? result = await _channel.Value.BasicGetAsync(queueName, autoAck: true);
                
                if (result == null)
                {
                    throw new InvalidOperationException($"No messages in queue: {queueName}");
                }

                // ? OCP: Используем абстракцию IMessageSerializer
                var json = Encoding.UTF8.GetString(result.Body.ToArray());
                var messageObj = _serializer.Deserialize<T>(json);

                if (messageObj == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize message from queue: {queueName}");
                }

                return messageObj;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error consuming message from queue: {QueueName}", queueName);
                throw;
            }
        }

        public void Subscribe<T>(string queueName, Action<T> handler)
        {
            try
            {
                _channel.Value.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                ).GetAwaiter().GetResult();

                var consumer = new AsyncEventingBasicConsumer(_channel.Value);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        
                        // ? OCP: Используем абстракцию IMessageSerializer
                        var messageObj = _serializer.Deserialize<T>(json);

                        if (messageObj != null)
                        {
                            handler(messageObj);
                        }

                        await _channel.Value.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error handling message from queue: {QueueName}", queueName);
                        // NACK message on error
                        await _channel.Value.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };

                _channel.Value.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer
                ).GetAwaiter().GetResult();

                _logger?.LogInformation("Subscribed to queue: {QueueName}", queueName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error subscribing to queue: {QueueName}", queueName);
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    if (_channel.IsValueCreated)
                    {
                        _channel.Value?.CloseAsync().GetAwaiter().GetResult();
                        _channel.Value?.Dispose();
                    }
                    _rabbitMqConnection?.Dispose();
                    _disposed = true;
                    
                    _logger?.LogInformation("RabbitMqMessageBus disposed");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error disposing RabbitMqMessageBus");
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
