using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Contracts.Messages;
using SteelDesignerEngineer.Services.Search.Messaging;

namespace SteelDesignerEngineer.Services.Search.Handlers;

internal sealed class SearchRpcServer : RabbitMqRpcServer
{
    private readonly IServiceProvider _serviceProvider;

    public SearchRpcServer(IConnection connection, string requestQueueName, IServiceProvider serviceProvider, ILogger logger)
        : base(connection, requestQueueName, logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<object?> ProcessRequestAsync(string messageType, string requestBody, string correlationId)
    {
        using var scope = _serviceProvider.CreateScope();

        return messageType switch
        {
            MessageQueues.SemanticSearchType => await HandleSemanticSearchAsync(requestBody, scope),
            MessageQueues.UpsertTextEmbeddingType => await HandleUpsertTextEmbeddingAsync(requestBody, scope),
            _ => new SemanticSearchResponse { Success = false, Message = $"Unknown message type: {messageType}" }
        };
    }

    private static T Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Invalid request body");

    private async Task<SemanticSearchResponse> HandleSemanticSearchAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<SemanticSearchRequest>(requestBody);
        if (string.IsNullOrWhiteSpace(request.Query))
            return new SemanticSearchResponse { Success = false, Message = "Query is empty" };

        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var http = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("search");

        var qdrantBase = cfg["Qdrant:BaseUrl"] ?? throw new InvalidOperationException("Qdrant:BaseUrl is required");
        var qdrantKey = cfg["Qdrant:ApiKey"]; // optional
        var collection = cfg["Qdrant:DefaultCollectionName"] ?? "MyCollection";

        var ollamaBase = cfg["Ollama:BaseUrl"] ?? throw new InvalidOperationException("Ollama:BaseUrl is required");
        var model = cfg["Ollama:Model"] ?? "nomic-embed-text";

        // Optional tuning knob (disabled by default to preserve pure semantic ranking)
        var scoreThreshold = cfg.GetValue<double?>("Search:ScoreThreshold");

        // 1) Embedding via Ollama
        var embReq = new { model, prompt = request.Query };

        using var embHttpReq = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(ollamaBase), "/api/embeddings"))
        {
            Content = new StringContent(JsonSerializer.Serialize(embReq), Encoding.UTF8, "application/json")
        };

        var ollamaUser = cfg["Ollama:Username"];
        var ollamaPass = cfg["Ollama:Password"];
        if (!string.IsNullOrWhiteSpace(ollamaUser) && ollamaPass != null)
        {
            var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ollamaUser}:{ollamaPass}"));
            embHttpReq.Headers.Authorization = new AuthenticationHeaderValue("Basic", raw);
        }

        using var embResp = await http.SendAsync(embHttpReq);
        embResp.EnsureSuccessStatusCode();
        var embJson = await embResp.Content.ReadAsStringAsync();

        var embedding = JsonDocument.Parse(embJson).RootElement.GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();

        // 2) Qdrant search (pure semantic)
        var qReq = new Dictionary<string, object?>
        {
            ["vector"] = embedding,
            ["limit"] = request.Limit <= 0 ? 10 : request.Limit,
            ["with_payload"] = true
        };

        if (scoreThreshold.HasValue)
            qReq["score_threshold"] = scoreThreshold.Value;

        using var qHttpReq = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(qdrantBase), $"/collections/{collection}/points/search"))
        {
            Content = new StringContent(JsonSerializer.Serialize(qReq), Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(qdrantKey))
            qHttpReq.Headers.Add("api-key", qdrantKey);

        using var qResp = await http.SendAsync(qHttpReq);
        qResp.EnsureSuccessStatusCode();

        var qJson = await qResp.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(qJson).RootElement;

        var items = new List<SearchResultItem>();
        var ids = new List<string>();

        foreach (var r in root.GetProperty("result").EnumerateArray())
        {
            var id = r.GetProperty("id").ToString();
            ids.Add(id);

            var score = r.GetProperty("score").GetDouble();

            var payload = new Dictionary<string, object?>();
            if (r.TryGetProperty("payload", out var payloadEl) && payloadEl.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in payloadEl.EnumerateObject())
                {
                    payload[p.Name] = p.Value.ValueKind switch
                    {
                        JsonValueKind.String => p.Value.GetString(),
                        JsonValueKind.Number => p.Value.TryGetInt64(out var l) ? l : p.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => p.Value.ToString()
                    };
                }
            }

            items.Add(new SearchResultItem { Id = id, Score = score, Payload = payload, Vector = null });
        }

        // If Qdrant returned empty payloads, try to fetch them via points/get.
        if (ids.Count > 0 && items.All(x => x.Payload.Count == 0))
        {
            var getReq = new
            {
                ids,
                with_payload = true,
                with_vector = false
            };

            using var getHttpReq = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(qdrantBase), $"/collections/{collection}/points"))
            {
                Content = new StringContent(JsonSerializer.Serialize(getReq), Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(qdrantKey))
                getHttpReq.Headers.Add("api-key", qdrantKey);

            using var getResp = await http.SendAsync(getHttpReq);
            if (getResp.IsSuccessStatusCode)
            {
                var getJson = await getResp.Content.ReadAsStringAsync();
                var getRoot = JsonDocument.Parse(getJson).RootElement;

                if (getRoot.TryGetProperty("result", out var getResultEl) && getResultEl.ValueKind == JsonValueKind.Array)
                {
                    var payloadById = new Dictionary<string, Dictionary<string, object?>>();
                    foreach (var p in getResultEl.EnumerateArray())
                    {
                        var pid = p.GetProperty("id").ToString();
                        var pp = new Dictionary<string, object?>();

                        if (p.TryGetProperty("payload", out var pPayload) && pPayload.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var kv in pPayload.EnumerateObject())
                            {
                                pp[kv.Name] = kv.Value.ValueKind switch
                                {
                                    JsonValueKind.String => kv.Value.GetString(),
                                    JsonValueKind.Number => kv.Value.TryGetInt64(out var l) ? l : kv.Value.GetDouble(),
                                    JsonValueKind.True => true,
                                    JsonValueKind.False => false,
                                    _ => kv.Value.ToString()
                                };
                            }
                        }

                        payloadById[pid] = pp;
                    }

                    foreach (var it in items)
                    {
                        if (payloadById.TryGetValue(it.Id, out var pp) && pp.Count > 0)
                        {
                            it.Payload = pp;
                        }
                    }
                }
            }
        }

        // Ensure title/snippet exist for UI
        foreach (var it in items)
        {
            var description = it.Payload.TryGetValue("description", out var dv) ? dv?.ToString() : null;
            if (string.IsNullOrWhiteSpace(description) && it.Payload.TryGetValue("Description", out var dv2))
                description = dv2?.ToString();

            var text = it.Payload.TryGetValue("text", out var tv) ? tv?.ToString() : null;
            if (string.IsNullOrWhiteSpace(text) && it.Payload.TryGetValue("Text", out var tv2))
                text = tv2?.ToString();

            var primaryText = !string.IsNullOrWhiteSpace(description) ? description : text;

            if (!it.Payload.ContainsKey("title") && !it.Payload.ContainsKey("Title"))
            {
                // Prefer city as title when available
                var city = it.Payload.TryGetValue("city", out var cv) ? cv?.ToString() : null;
                if (string.IsNullOrWhiteSpace(city) && it.Payload.TryGetValue("City", out var cv2))
                    city = cv2?.ToString();

                var title = !string.IsNullOrWhiteSpace(city)
                    ? city
                    : (!string.IsNullOrWhiteSpace(primaryText)
                        ? (primaryText.Length <= 80 ? primaryText : primaryText[..80] + "…")
                        : it.Id);

                it.Payload["title"] = title;
            }

            if (!it.Payload.ContainsKey("snippet") && !it.Payload.ContainsKey("Snippet") && !string.IsNullOrWhiteSpace(primaryText))
            {
                var snippet = primaryText.Length <= 240 ? primaryText : primaryText[..240] + "…";
                it.Payload["snippet"] = snippet;
            }
        }

        return new SemanticSearchResponse { Success = true, Items = items };
    }

    private async Task<UpsertTextEmbeddingResponse> HandleUpsertTextEmbeddingAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<UpsertTextEmbeddingRequest>(requestBody);
        if (string.IsNullOrWhiteSpace(request.Text))
            return new UpsertTextEmbeddingResponse { Success = false, Message = "Text is empty" };

        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var http = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("search");

        var qdrantBase = cfg["Qdrant:BaseUrl"] ?? throw new InvalidOperationException("Qdrant:BaseUrl is required");
        var qdrantKey = cfg["Qdrant:ApiKey"]; // optional
        var collection = request.CollectionName
            ?? cfg["Qdrant:DefaultCollectionName"]
            ?? "MyCollection";

        var ollamaBase = cfg["Ollama:BaseUrl"] ?? throw new InvalidOperationException("Ollama:BaseUrl is required");
        var model = cfg["Ollama:Model"] ?? "nomic-embed-text";

        // 1) Embedding via Ollama
        var embReq = new { model, prompt = request.Text };

        using var embHttpReq = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(ollamaBase), "/api/embeddings"))
        {
            Content = new StringContent(JsonSerializer.Serialize(embReq), Encoding.UTF8, "application/json")
        };

        var ollamaUser = cfg["Ollama:Username"];
        var ollamaPass = cfg["Ollama:Password"];
        if (!string.IsNullOrWhiteSpace(ollamaUser) && ollamaPass != null)
        {
            var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ollamaUser}:{ollamaPass}"));
            embHttpReq.Headers.Authorization = new AuthenticationHeaderValue("Basic", raw);
        }

        using var embResp = await http.SendAsync(embHttpReq);
        embResp.EnsureSuccessStatusCode();
        var embJson = await embResp.Content.ReadAsStringAsync();

        var embedding = JsonDocument.Parse(embJson).RootElement.GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();

        // 2) Upsert into Qdrant
        var pointId = string.IsNullOrWhiteSpace(request.PointId) ? Guid.NewGuid().ToString("N") : request.PointId;
        var payload = request.Payload ?? new Dictionary<string, object?>
        {
            ["text"] = request.Text,
            ["createdAt"] = DateTime.UtcNow.ToString("O")
        };

        var upsertReq = new
        {
            points = new[]
            {
                new
                {
                    id = pointId,
                    vector = embedding,
                    payload
                }
            }
        };

        using var qHttpReq = new HttpRequestMessage(HttpMethod.Put, new Uri(new Uri(qdrantBase), $"/collections/{collection}/points"))
        {
            Content = new StringContent(JsonSerializer.Serialize(upsertReq), Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(qdrantKey))
            qHttpReq.Headers.Add("api-key", qdrantKey);

        using var qResp = await http.SendAsync(qHttpReq);
        var qJson = await qResp.Content.ReadAsStringAsync();

        if (!qResp.IsSuccessStatusCode)
        {
            return new UpsertTextEmbeddingResponse
            {
                Success = false,
                Message = $"Qdrant error: {(int)qResp.StatusCode}",
                CollectionName = collection,
                PointId = pointId,
                QdrantResponseJson = qJson,
                Embedding = embedding
            };
        }

        return new UpsertTextEmbeddingResponse
        {
            Success = true,
            Message = "Upsert successful",
            CollectionName = collection,
            PointId = pointId,
            QdrantResponseJson = qJson,
            Embedding = embedding
        };
    }
}
