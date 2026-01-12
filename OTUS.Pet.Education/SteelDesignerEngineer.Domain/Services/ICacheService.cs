namespace SteelDesignerEngineer.Domain.Services
{
    /// <summary>
    /// Интерфейс кэш-сервиса
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Получить данные из кэша
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Записать данные в кэш
        /// </summary>
        /// <param name="key">Ключ в Redis</param>
        /// <param name="value">Значение в Redis</param>
        /// <param name="expiration">Промежуток времени</param>
        /// <returns></returns>
        Task SetAsync(string key, string value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Записать данные в кэш с ограничением по времени жизни (TTL)
        /// </summary>
        /// <param name="key">Ключ в Redis</param>
        /// <param name="value">Значение в Redis</param>
        /// <param name="expiration">Время жизни в кеше</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default);

        /// <summary>
        /// Стереть значения в Redis
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Стереть значения в Redis (старое имя для совместимости)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RemoveDataAsync(string key);

        /// <summary>
        /// Содержит данные по ключу или нет
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Возращает bool содержит(true) или нет(false)</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
