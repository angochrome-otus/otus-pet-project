using System.IO;
using System.Text.RegularExpressions;
using SteelDesignerEngineer.WPF.Models;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Реализация сервиса для работы с локальными файлами
/// </summary>
public class FileService : IFileService
{
    public async Task<HtmlPageModel?> LoadPageFromFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var content = await File.ReadAllTextAsync(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            
            // Извлечь title из HTML
            var title = ExtractTitle(content) ?? fileName;

            return new HtmlPageModel
            {
                PageName = fileName,
                Title = title,
                Content = content,
                FilePath = filePath,
                LastModified = File.GetLastWriteTime(filePath)
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка загрузки файла {filePath}: {ex.Message}", ex);
        }
    }

    public async Task SavePageToFileAsync(HtmlPageModel page, string filePath)
    {
        try
        {
            // Обновить title в HTML
            var content = UpdateTitle(page.Content, page.Title);
            
            await File.WriteAllTextAsync(filePath, content);
            page.LastModified = DateTime.Now;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка сохранения файла {filePath}: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> GetWwwrootHtmlFilesAsync(string wwwrootPath)
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(wwwrootPath))
            {
                return new List<string>();
            }

            return Directory.GetFiles(wwwrootPath, "*.html", SearchOption.AllDirectories)
                .ToList();
        });
    }

    private string? ExtractTitle(string htmlContent)
    {
        var match = Regex.Match(htmlContent, @"<title>(.*?)</title>", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private string UpdateTitle(string htmlContent, string newTitle)
    {
        return Regex.Replace(
            htmlContent,
            @"<title>(.*?)</title>",
            $"<title>{newTitle}</title>",
            RegexOptions.IgnoreCase
        );
    }
}
