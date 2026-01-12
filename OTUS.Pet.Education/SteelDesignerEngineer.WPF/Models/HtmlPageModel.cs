namespace SteelDesignerEngineer.WPF.Models;

/// <summary>
/// Модель HTML страницы для редактирования
/// </summary>
public class HtmlPageModel
{
    public Guid Id { get; set; }
    public string PageName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string FilePath { get; set; } = string.Empty;
}
