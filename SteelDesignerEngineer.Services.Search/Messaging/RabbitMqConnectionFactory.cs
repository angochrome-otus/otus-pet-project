using RabbitMQ.Client;

namespace SteelDesignerEngineer.Services.Search.Messaging;

internal static class RabbitMqConnectionFactory
{
    public static IConnection Create(string connectionString)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection();
    }
}
