using SteelDesignerEngineer.WPF.Models;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Интерфейс для работы с локальными файлами
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Загрузить HTML страницу из файла
    /// </summary>
    Task<HtmlPageModel?> LoadPageFromFileAsync(string filePath);
    
    /// <summary>
    /// Сохранить HTML страницу в файл
    /// </summary>
    Task SavePageToFileAsync(HtmlPageModel page, string filePath);
    
    /// <summary>
    /// Получить список HTML файлов в директории wwwroot
    /// </summary>
    Task<List<string>> GetWwwrootHtmlFilesAsync(string wwwrootPath);
}
