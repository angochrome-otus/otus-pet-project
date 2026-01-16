using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Services.Auth.Messaging;

internal abstract class RabbitMqRpcServer : IDisposable
{
    protected readonly IConnection Connection;
    protected readonly IChannel Channel;
    protected readonly ILogger Logger;
    protected readonly string RequestQueueName;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    protected RabbitMqRpcServer(IConnection connection, string requestQueueName, ILogger logger)
    {
        Connection = connection;
        RequestQueueName = requestQueueName;
        Logger = logger;

        Channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
        Channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false).GetAwaiter().GetResult();

        Logger.LogInformation("RabbitMQ RPC Server initialized for queue: {QueueName}", requestQueueName);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Channel.QueueDeclareAsync(
            queue: RequestQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.ReceivedAsync += OnRequestReceived;

        await Channel.BasicConsumeAsync(
            queue: RequestQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        Logger.LogInformation("Started consuming requests from queue: {QueueName}", RequestQueueName);
    }

    private async Task OnRequestReceived(object sender, BasicDeliverEventArgs ea)
    {
        var correlationId = ea.BasicProperties.CorrelationId;
        var replyTo = ea.BasicProperties.ReplyTo;
        var messageType = ea.BasicProperties.Headers?.ContainsKey("X-Message-Type") == true
            ? Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["X-Message-Type"])
            : "unknown";

        try
        {
            var requestBody = Encoding.UTF8.GetString(ea.Body.ToArray());
            var response = await ProcessRequestAsync(messageType, requestBody, correlationId);

            if (!string.IsNullOrEmpty(replyTo) && response != null)
                await SendResponseAsync(replyTo, correlationId, response);

            await Channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing request, CorrelationId: {CorrelationId}", correlationId);

            if (!string.IsNullOrEmpty(replyTo))
            {
                var errorResponse = new { Success = false, Message = ex.Message };
                await SendResponseAsync(replyTo, correlationId, errorResponse);
            }

            await Channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    protected abstract Task<object?> ProcessRequestAsync(string messageType, string requestBody, string correlationId);

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
                body: responseBody);
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
    }
}
