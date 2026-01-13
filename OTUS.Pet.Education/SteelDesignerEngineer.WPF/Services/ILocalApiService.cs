namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Интерфейс для управления локальным API сервером
/// </summary>
public interface ILocalApiService
{
    /// <summary>
    /// Запустить локальный API сервер
    /// </summary>
    Task<bool> StartAsync();
    
    /// <summary>
    /// Остановить локальный API сервер
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Проверить, запущен ли локальный API
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Получить URL локального API
    /// </summary>
    string GetLocalApiUrl();
}
