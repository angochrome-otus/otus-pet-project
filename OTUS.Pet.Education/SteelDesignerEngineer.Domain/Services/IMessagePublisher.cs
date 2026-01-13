using System.Threading.Tasks;

namespace SteelDesignerEngineer.Domain.Services;

/// <summary>
/// Interface for publishing messages to a message broker
/// ISP: Segregated from consumer operations
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publish a message to the specified queue
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name</param>
    /// <param name="message">Message to publish</param>
    Task PublishAsync<T>(string queueName, T message);
}
