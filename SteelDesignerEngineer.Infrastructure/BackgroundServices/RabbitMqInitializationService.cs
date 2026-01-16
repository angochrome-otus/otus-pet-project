using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Domain.Constants;
using SteelDesignerEngineer.Infrastructure.Messaging;

namespace SteelDesignerEngineer.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service to initialize RabbitMQ queues on startup
    /// Ensures all queues are created when application starts
    /// </summary>
    public class RabbitMqInitializationService : IHostedService
    {
        private readonly IRabbitMqConnectionExtended _rabbitMqConnection;
        private readonly ILogger<RabbitMqInitializationService> _logger;

        public RabbitMqInitializationService(
            IRabbitMqConnectionExtended rabbitMqConnection,
            ILogger<RabbitMqInitializationService> logger)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("?? Initializing RabbitMQ queues...");

                // Get all queue names from constants
                var queueNames = new[]
                {
                    // Authentication queues
                    RabbitMqQueues.AuthLoginRequest,
                    RabbitMqQueues.AuthLoginResponse,
                    RabbitMqQueues.AuthRegisterRequest,
                    RabbitMqQueues.AuthRegisterResponse,
                    RabbitMqQueues.AuthLogoutRequest,
                    
                    // User management queues
                    RabbitMqQueues.UserGetRequest,
                    RabbitMqQueues.UserGetResponse,
                    RabbitMqQueues.UserUpdateRequest,
                    RabbitMqQueues.UserUpdateResponse,
                    
                    // Session management queues
                    RabbitMqQueues.SessionCreateRequest,
                    RabbitMqQueues.SessionCreateResponse,
                    RabbitMqQueues.SessionValidateRequest,
                    RabbitMqQueues.SessionValidateResponse,
                    RabbitMqQueues.SessionDeleteRequest,
                    
                    // Page content queues
                    RabbitMqQueues.PageGetRequest,
                    RabbitMqQueues.PageGetResponse,
                    RabbitMqQueues.PageUpdateRequest,
                    RabbitMqQueues.PageUpdateResponse,
                    
                    // Events
                    RabbitMqQueues.UserLoggedInEvent,
                    RabbitMqQueues.UserLoggedOutEvent,
                    RabbitMqQueues.UserRegisteredEvent
                };

                var connection = await _rabbitMqConnection.GetConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                foreach (var queueName in queueNames)
                {
                    await channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    _logger.LogDebug("? Queue declared: {QueueName}", queueName);
                }

                _logger.LogInformation("? RabbitMQ queues initialized successfully ({Count} queues)", queueNames.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Failed to initialize RabbitMQ queues");
                // Don't throw - let the application start even if RabbitMQ is not available
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQ initialization service stopped");
            return Task.CompletedTask;
        }
    }
}
