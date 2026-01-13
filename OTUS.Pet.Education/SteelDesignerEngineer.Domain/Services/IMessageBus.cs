using System;
using System.Threading.Tasks;

namespace SteelDesignerEngineer.Domain.Services
{
    /// <summary>
    /// Combined interface for message bus operations
    /// ISP: Inherits from segregated interfaces for backward compatibility
    /// Use IMessagePublisher or IMessageConsumer directly when possible
    /// </summary>
    public interface IMessageBus : IMessagePublisher, IMessageConsumer
    {
        // Empty - all methods inherited from IMessagePublisher and IMessageConsumer
    }
}