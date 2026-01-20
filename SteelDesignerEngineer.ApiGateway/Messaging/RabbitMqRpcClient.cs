using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.ApiGateway.Messaging;

public sealed class RabbitMqRpcClient : IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _replyQueueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbacks = new();
    private readonly ILogger<RabbitMqRpcClient> _logger;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    public RabbitMqRpcClient(IConnection connection, ILogger<RabbitMqRpcClient> logger)
    {
        _connection = connection;
        _logger = logger;

        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        _replyQueueName = _channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true,
            arguments: null).GetAwaiter().GetResult().QueueName;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnReplyReceived;

        _channel.BasicConsumeAsync(
            queue: _replyQueueName,
            autoAck: true,
            consumer: consumer).GetAwaiter().GetResult();
    }

    public async Task<TResponse?> CallAsync<TRequest, TResponse>(
        string requestQueue,
        TRequest request,
        string messageType,
        TimeSpan? timeout = null)
        where TRequest : class
        where TResponse : class
    {
        timeout ??= TimeSpan.FromSeconds(30);

        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _callbacks[correlationId] = tcs;

        try
        {
            await _channelLock.WaitAsync();

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = _replyQueueName,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Headers = new Dictionary<string, object?>
                {
                    { "X-Message-Type", messageType },
                    { "X-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
                }
            };

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: requestQueue,
                mandatory: false,
                basicProperties: props,
                body: body);
        }
        finally
        {
            _channelLock.Release();
        }

        using var cts = new CancellationTokenSource(timeout.Value);
        var registration = cts.Token.Register(() => tcs.TrySetCanceled());

        try
        {
            var json = await tcs.Task;
            return JsonSerializer.Deserialize<TResponse>(json);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RPC timeout to {Queue} type {Type}", requestQueue, messageType);
            return null;
        }
        finally
        {
            registration.Dispose();
            _callbacks.TryRemove(correlationId, out var _);
        }
    }

    private Task OnReplyReceived(object sender, BasicDeliverEventArgs ea)
    {
        if (!_callbacks.TryGetValue(ea.BasicProperties.CorrelationId, out var tcs))
            return Task.CompletedTask;

        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        tcs.TrySetResult(json);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _channelLock?.Dispose();
    }
}
