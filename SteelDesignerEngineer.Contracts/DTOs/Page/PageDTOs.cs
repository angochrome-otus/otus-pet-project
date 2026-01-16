namespace SteelDesignerEngineer.Contracts.DTOs.Page;

public class PageContentDto
{
    public string Id { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/html";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GetPageRequest
{
    public string? PageId { get; set; }
    public string? PageName { get; set; }
}

public class GetPageResponse
{
    public bool Success { get; set; }
    public PageContentDto? Page { get; set; }
    public string? Message { get; set; }
}

public class UpdatePageRequest
{
    public string PageId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Content { get; set; }
}

public class UpdatePageResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
