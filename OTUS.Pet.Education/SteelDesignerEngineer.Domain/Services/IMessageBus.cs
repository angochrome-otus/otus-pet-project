using System;
using System.Threading.Tasks;

namespace SteelDesignerEngineer.Domain.Services
{
    /// <summary>
    /// Интерфейс для шины сообщений (DIP)
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Записать сообщение в очередь с сериализацией
        /// </summary>
        /// <typeparam name="T">Принимает в себя классы поэтому обобщил</typeparam>
        /// <param name="queueName">Имя очереди</param>
        /// <param name="message">Сообщение</param>
        /// <returns>Записал значение в очередь</returns>
        Task PublishAsync<T>(string queueName, T message);
        /// <summary>
        /// Получить сообщение из очереди
        /// </summary>
        /// <typeparam name="T">Передача типа в метод, класса</typeparam>
        /// <param name="queueName">Имя очереди</param>
        /// <param name="timeout">Пауза или опциональный таймаут для приложения</param>
        /// <returns>Получил значение из очереди после паузы</returns>
        Task<T> ConsumeAsync<T>(string queueName, TimeSpan timeout);
        /// <summary>
        /// Подписка на сообщения из очереди
        /// </summary>
        /// <typeparam name="T">Передача типа в метод, класса</typeparam>
        /// <param name="queueName">Имя очереди</param>
        /// <param name="handler">обработчик для очереди</param>
        void Subscribe<T>(string queueName, Action<T> handler);
    }
}