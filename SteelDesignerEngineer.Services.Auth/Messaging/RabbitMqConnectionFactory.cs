using RabbitMQ.Client;

namespace SteelDesignerEngineer.Services.Auth.Messaging;

internal static class RabbitMqConnectionFactory
{
    public static IConnection Create(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("RabbitMQ connection string is not configured.");

        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
        };

        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }
}
