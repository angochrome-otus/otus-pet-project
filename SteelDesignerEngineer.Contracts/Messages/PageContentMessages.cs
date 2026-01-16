namespace SteelDesignerEngineer.Contracts.Messages;

// ==========================================
// Request-Reply Messages for Page Content Service
// ==========================================

/// <summary>
/// Get page by name request
/// </summary>
public record GetPageByNameRequest : BaseMessage
{
    public required string PageName { get; init; }
}

/// <summary>
/// Get page by name response
/// </summary>
public record GetPageByNameResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? PageId { get; init; }
    public string? PageName { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    public DateTime? LastModified { get; init; }
}

/// <summary>
/// Get page by ID request
/// </summary>
public record GetPageByIdRequest : BaseMessage
{
    public required string PageId { get; init; }
}

/// <summary>
/// Get page by ID response
/// </summary>
public record GetPageByIdResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? PageId { get; init; }
    public string? PageName { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    public DateTime? LastModified { get; init; }
}

/// <summary>
/// Get all pages request
/// </summary>
public record GetAllPagesRequest : BaseMessage
{
}

/// <summary>
/// Get all pages response
/// </summary>
public record GetAllPagesResponse : BaseMessage
{
    public bool Success { get; init; }
    public List<PageInfo> Pages { get; init; } = new();
    public int TotalCount { get; init; }
}

/// <summary>
/// Page information
/// </summary>
public record PageInfo
{
    public string PageId { get; init; } = string.Empty;
    public string PageName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? MetaDescription { get; init; }
    public DateTime LastModified { get; init; }
}

/// <summary>
/// Create page request
/// </summary>
public record CreatePageRequest : BaseMessage
{
    public required string PageName { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
}

/// <summary>
/// Create page response
/// </summary>
public record CreatePageResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? PageId { get; init; }
}

/// <summary>
/// Update page request
/// </summary>
public record UpdatePageRequest : BaseMessage
{
    public required string PageId { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
}

/// <summary>
/// Update page response
/// </summary>
public record UpdatePageResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Delete page request
/// </summary>
public record DeletePageRequest : BaseMessage
{
    public required string PageName { get; init; }
}

/// <summary>
/// Delete page response
/// </summary>
public record DeletePageResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}
