namespace SteelDesignerEngineer.Contracts.Messages;

// ==========================================
// Request-Reply Messages for Search Service
// ==========================================

public record SemanticSearchRequest : BaseMessage
{
    public required string Query { get; init; }
    public int Limit { get; init; } = 10;
}

public record SemanticSearchResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public List<SearchResultItem> Items { get; init; } = new();
}

public record SearchResultItem
{
    public string Id { get; init; } = string.Empty;
    public double Score { get; init; }

    /// <summary>
    /// Qdrant point payload (stored document fields)
    /// </summary>
    public Dictionary<string, object?> Payload { get; set; } = new();

    /// <summary>
    /// Optional point vector (only if requested from Qdrant)
    /// </summary>
    public float[]? Vector { get; init; }
}

public record UpsertTextEmbeddingRequest : BaseMessage
{
    public required string Text { get; init; }
    public string? CollectionName { get; init; }
    public string? PointId { get; init; }
    public Dictionary<string, object?>? Payload { get; init; }
}

public record UpsertTextEmbeddingResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? CollectionName { get; init; }
    public string? PointId { get; init; }
    public string? QdrantResponseJson { get; init; }
    public float[]? Embedding { get; init; }
}
