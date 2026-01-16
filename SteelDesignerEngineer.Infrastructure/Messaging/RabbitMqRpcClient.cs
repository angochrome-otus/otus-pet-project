using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ Request-Reply client for synchronous communication between microservices
/// Implements Request-Reply pattern with correlation IDs
/// </summary>
public class RabbitMqRpcClient : IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _replyQueueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private readonly ILogger<RabbitMqRpcClient> _logger;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    public RabbitMqRpcClient(IConnection connection, ILogger<RabbitMqRpcClient> logger)
    {
        _connection = connection;
        _logger = logger;
        
        // Create channel and declare reply queue
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        
        // Declare exclusive reply queue for this client
        _replyQueueName = _channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true,
            arguments: null
        ).GetAwaiter().GetResult().QueueName;

        // Start consuming replies
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnReplyReceived;
        
        _channel.BasicConsumeAsync(
            queue: _replyQueueName,
            autoAck: true,
            consumer: consumer
        ).GetAwaiter().GetResult();

        _logger.LogInformation("RabbitMQ RPC Client initialized with reply queue: {QueueName}", _replyQueueName);
    }

    /// <summary>
    /// Send request and wait for response (Request-Reply pattern)
    /// </summary>
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
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);

        try
        {
            await _channelLock.WaitAsync();

            var messageBody = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = new BasicProperties
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
                mandatory: true,
                basicProperties: properties,
                body: body
            );

            _logger.LogDebug("Sent RPC request to {Queue}, CorrelationId: {CorrelationId}, Type: {MessageType}",
                requestQueue, correlationId, messageType);
        }
        finally
        {
            _channelLock.Release();
        }

        // Wait for response with timeout
        using var cts = new CancellationTokenSource(timeout.Value);
        var registration = cts.Token.Register(() =>
        {
            _callbackMapper.TryRemove(correlationId, out _);
            tcs.TrySetCanceled();
        });

        try
        {
            var responseJson = await tcs.Task;
            var response = JsonSerializer.Deserialize<TResponse>(responseJson);

            _logger.LogDebug("Received RPC response, CorrelationId: {CorrelationId}", correlationId);
            return response;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RPC request timeout, CorrelationId: {CorrelationId}, Queue: {Queue}",
                correlationId, requestQueue);
            return null;
        }
        finally
        {
            registration.Dispose();
        }
    }

    /// <summary>
    /// Handle incoming reply messages
    /// </summary>
    private Task OnReplyReceived(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
            {
                _logger.LogWarning("Received reply with unknown CorrelationId: {CorrelationId}",
                    ea.BasicProperties.CorrelationId);
                return Task.CompletedTask;
            }

            var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());
            tcs.TrySetResult(responseJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RPC reply");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _channelLock?.Dispose();
        _logger.LogInformation("RabbitMQ RPC Client disposed");
    }
}
