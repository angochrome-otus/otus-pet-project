using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ Request-Reply server for handling incoming requests in microservices
/// </summary>
public abstract class RabbitMqRpcServer : IDisposable
{
    protected readonly IConnection Connection;
    protected readonly IChannel Channel;
    protected readonly ILogger Logger;
    protected readonly string RequestQueueName;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    protected RabbitMqRpcServer(
        IConnection connection,
        string requestQueueName,
        ILogger logger)
    {
        Connection = connection;
        RequestQueueName = requestQueueName;
        Logger = logger;

        Channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
        
        // Set prefetch count for fair dispatch
        Channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false).GetAwaiter().GetResult();

        Logger.LogInformation("RabbitMQ RPC Server initialized for queue: {QueueName}", requestQueueName);
    }

    /// <summary>
    /// Start consuming requests
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // Ensure queue exists
        await Channel.QueueDeclareAsync(
            queue: RequestQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.ReceivedAsync += OnRequestReceived;

        await Channel.BasicConsumeAsync(
            queue: RequestQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken
        );

        Logger.LogInformation("Started consuming requests from queue: {QueueName}", RequestQueueName);
    }

    /// <summary>
    /// Handle incoming request
    /// </summary>
    private async Task OnRequestReceived(object sender, BasicDeliverEventArgs ea)
    {
        var correlationId = ea.BasicProperties.CorrelationId;
        var replyTo = ea.BasicProperties.ReplyTo;
        var messageType = ea.BasicProperties.Headers?.ContainsKey("X-Message-Type") == true
            ? Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["X-Message-Type"])
            : "unknown";

        try
        {
            Logger.LogDebug("Processing request, CorrelationId: {CorrelationId}, Type: {MessageType}",
                correlationId, messageType);

            var requestBody = Encoding.UTF8.GetString(ea.Body.ToArray());
            
            // Process request (implemented by derived class)
            var response = await ProcessRequestAsync(messageType, requestBody, correlationId);

            // Send response if replyTo is specified
            if (!string.IsNullOrEmpty(replyTo) && response != null)
            {
                await SendResponseAsync(replyTo, correlationId, response);
            }

            // Acknowledge message
            await Channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

            Logger.LogDebug("Request processed successfully, CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing request, CorrelationId: {CorrelationId}", correlationId);

            // Send error response
            if (!string.IsNullOrEmpty(replyTo))
            {
                var errorResponse = new { Success = false, Message = ex.Message };
                await SendResponseAsync(replyTo, correlationId, errorResponse);
            }

            // Negative acknowledge - requeue the message
            await Channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    /// <summary>
    /// Process incoming request - must be implemented by derived class
    /// </summary>
    protected abstract Task<object?> ProcessRequestAsync(string messageType, string requestBody, string correlationId);

    /// <summary>
    /// Send response back to client
    /// </summary>
    protected async Task SendResponseAsync(string replyTo, string correlationId, object response)
    {
        await _channelLock.WaitAsync();
        try
        {
            var responseJson = JsonSerializer.Serialize(response);
            var responseBody = Encoding.UTF8.GetBytes(responseJson);

            var replyProperties = new BasicProperties
            {
                CorrelationId = correlationId,
                ContentType = "application/json"
            };

            await Channel.BasicPublishAsync(
                exchange: "",
                routingKey: replyTo,
                mandatory: false,
                basicProperties: replyProperties,
                body: responseBody
            );

            Logger.LogDebug("Sent response to {ReplyTo}, CorrelationId: {CorrelationId}", replyTo, correlationId);
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public virtual void Dispose()
    {
        Channel?.Dispose();
        _channelLock?.Dispose();
        Logger.LogInformation("RabbitMQ RPC Server disposed for queue: {QueueName}", RequestQueueName);
    }
}
