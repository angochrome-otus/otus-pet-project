using SteelDesignerEngineer.WPF.Models;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Интерфейс для работы с API портала
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Получить все страницы
    /// </summary>
    Task<List<HtmlPageModel>> GetPagesAsync();
    
    /// <summary>
    /// Получить страницу по имени
    /// </summary>
    Task<HtmlPageModel?> GetPageByNameAsync(string pageName);
    
    /// <summary>
    /// Создать новую страницу
    /// </summary>
    Task<HtmlPageModel> CreatePageAsync(HtmlPageModel page);
    
    /// <summary>
    /// Обновить страницу
    /// </summary>
    Task<HtmlPageModel> UpdatePageAsync(HtmlPageModel page);
    
    /// <summary>
    /// Удалить страницу
    /// </summary>
    Task DeletePageAsync(string pageName);
    
    /// <summary>
    /// Проверить доступность API
    /// </summary>
    Task<bool> CheckApiHealthAsync();
    
    /// <summary>
    /// Новые методы для браузера портала
    /// </summary>
    Task<bool> CheckApiAvailabilityAsync();
    string GetBaseUrl();
}
