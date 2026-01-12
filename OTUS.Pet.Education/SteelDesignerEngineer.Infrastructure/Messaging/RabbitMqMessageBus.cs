using SteelDesignerEngineer.Domain.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace SteelDesignerEngineer.Infrastructure.Messaging
{
    /// <summary>
    /// Реализация шины сообщений с использованием RabbitMQ с DIP
    /// </summary>
    public class RabbitMqMessageBus : IMessageBus, IDisposable
    {
        private readonly IRabbitMqConnection _rabbitMqConnection;
        private readonly IChannel _channel;
        private bool _disposed;

        public RabbitMqMessageBus(IConnection connection, IRabbitMqConnection rabbitMqConnection)
        {
            _rabbitMqConnection = rabbitMqConnection ?? throw new ArgumentNullException(nameof(rabbitMqConnection));
            _channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task PublishAsync<T>(string queueName, T message)
        {
            // Объявляем очередь (создаётся, если не существует)
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Сериализуем сообщение
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            // Публикуем с настройками для персистентности
            var properties = new BasicProperties
            {
                Persistent = true
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }

        public async Task<T> ConsumeAsync<T>(string queueName, TimeSpan timeout )
        {
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            BasicGetResult? result = await _channel.BasicGetAsync(queueName, autoAck: true);
            
            

            var json = Encoding.UTF8.GetString(result.Body.ToArray());
            var messageObj = JsonSerializer.Deserialize<T>(json);

            return messageObj;
        }

        public void Subscribe<T>(string queueName, Action<T> handler)
        {
            _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            ).GetAwaiter().GetResult();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var messageObj = JsonSerializer.Deserialize<T>(json);

                if (messageObj != null)
                {
                    handler(messageObj);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            ).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _channel?.CloseAsync().GetAwaiter().GetResult();
                _channel?.Dispose();
                _rabbitMqConnection?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        
    }
}
