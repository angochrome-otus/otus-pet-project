using System;
using System.Threading.Tasks;

namespace SteelDesignerEngineer.Domain.Services;

/// <summary>
/// Interface for consuming messages from a message broker
/// ISP: Segregated from publisher operations
/// </summary>
public interface IMessageConsumer
{
    /// <summary>
    /// Consume a single message from the queue
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name</param>
    /// <param name="timeout">Timeout for waiting</param>
    /// <returns>Consumed message</returns>
    Task<T> ConsumeAsync<T>(string queueName, TimeSpan timeout);
    
    /// <summary>
    /// Subscribe to messages from the queue with a handler
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name</param>
    /// <param name="handler">Message handler action</param>
    void Subscribe<T>(string queueName, Action<T> handler);
}
