namespace SteelDesignerEngineer.WebApi.BackgroundServices
{
    /// <summary>
    /// Background service для обработки сообщений из RabbitMQ
    /// </summary>
    public class MessageHandlerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageHandlerHostedService> _logger;

        public MessageHandlerHostedService(
            IServiceProvider serviceProvider,
            ILogger<MessageHandlerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MessageHandlerHostedService запускается...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var services = scope.ServiceProvider;

                // Получаем обработчики
                var healthHandler = services.GetRequiredService<SteelDesignerEngineer.WebApi.MessageHandlers.HealthMessageHandler>();

                // Подписываемся на сообщения
                healthHandler.Start();

                _logger.LogInformation("Все обработчики сообщений успешно запущены и подписаны");

                // Держим сервис активным
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при запуске MessageHandlerHostedService");
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MessageHandlerHostedService останавливается...");
            return base.StopAsync(cancellationToken);
        }
    }
}
