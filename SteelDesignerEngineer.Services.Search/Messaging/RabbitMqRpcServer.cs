using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SteelDesignerEngineer.Services.Search.Messaging;

internal abstract class RabbitMqRpcServer : IDisposable
{
    private readonly IModel _channel;
    private readonly string _queueName;
    protected readonly ILogger Logger;

    protected RabbitMqRpcServer(IConnection connection, string queueName, ILogger logger)
    {
        _queueName = queueName;
        Logger = logger;
        _channel = connection.CreateModel();

        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(0, 10, false);
    }

    public Task StartAsync(CancellationToken ct)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnReceivedAsync;

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        Logger.LogInformation("RPC server consuming: {Queue}", _queueName);

        ct.Register(() => Dispose());
        return Task.CompletedTask;
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var props = ea.BasicProperties;
        var replyProps = _channel.CreateBasicProperties();
        replyProps.CorrelationId = props.CorrelationId;

        var body = ea.Body.ToArray();
        var requestBody = Encoding.UTF8.GetString(body);

        var messageType = props.Headers != null && props.Headers.TryGetValue("X-Message-Type", out var mt)
            ? Encoding.UTF8.GetString((byte[])mt)
            : string.Empty;

        try
        {
            var result = await ProcessRequestAsync(messageType, requestBody, props.CorrelationId ?? string.Empty);
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));

            _channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "RPC processing failed: {Type}", messageType);
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Success = false, Message = ex.Message }));
            _channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        }
    }

    protected abstract Task<object?> ProcessRequestAsync(string messageType, string requestBody, string correlationId);

    public void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _channel?.Dispose(); } catch { }
    }
}
