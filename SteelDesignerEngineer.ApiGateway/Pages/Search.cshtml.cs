using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Pages;

public sealed class SearchModel : PageModel
{
    private readonly ISearchServiceClient _search;

    public SearchModel(ISearchServiceClient search)
    {
        _search = search;
    }

    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Query { get; set; }

    public bool HasSearched { get; private set; }
    public string? ErrorMessage { get; private set; }

    public int ResultsCount => Items.Count;

    public List<SearchItemView> Items { get; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            HasSearched = false;
            return;
        }

        HasSearched = true;

        var res = await _search.SemanticSearchAsync(new SemanticSearchRequest
        {
            Query = Query,
            Limit = 10
        }, cancellationToken);

        if (res == null)
        {
            ErrorMessage = "Search service unavailable";
            return;
        }

        if (!res.Success)
        {
            ErrorMessage = res.Message ?? "Search failed";
            return;
        }

        foreach (var i in res.Items)
        {
            var title = TryGetString(i.Payload, "title") ?? TryGetString(i.Payload, "Title");
            var pageName = TryGetString(i.Payload, "pageName") ?? TryGetString(i.Payload, "PageName");

            // Prefer domain payload fields
            var description = TryGetString(i.Payload, "description") ?? TryGetString(i.Payload, "Description");
            var snippet = TryGetString(i.Payload, "snippet") ?? TryGetString(i.Payload, "Snippet") ?? description;

            // Fallbacks when index payload is minimal
            var text = TryGetString(i.Payload, "text") ?? TryGetString(i.Payload, "Text");

            if (string.IsNullOrWhiteSpace(title))
            {
                var city = TryGetString(i.Payload, "city") ?? TryGetString(i.Payload, "City");
                var country = TryGetString(i.Payload, "country") ?? TryGetString(i.Payload, "Country");

                if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(country))
                    title = $"{city}, {country}";
                else if (!string.IsNullOrWhiteSpace(city))
                    title = city;
                else if (!string.IsNullOrWhiteSpace(text))
                    title = text.Length <= 80 ? text : text[..80] + "...";
                else if (!string.IsNullOrWhiteSpace(description))
                    title = description.Length <= 80 ? description : description[..80] + "...";
            }

            if (string.IsNullOrWhiteSpace(snippet) && !string.IsNullOrWhiteSpace(text))
                snippet = text.Length <= 240 ? text : text[..240] + "...";

            title ??= i.Id;

            Items.Add(new SearchItemView
            {
                Id = i.Id,
                Score = i.Score,
                Title = title,
                PageName = pageName,
                Snippet = snippet
            });
        }
    }

    private static string? TryGetString(Dictionary<string, object?> dict, string key)
        => dict.TryGetValue(key, out var v) ? v?.ToString() : null;

    public sealed class SearchItemView
    {
        public string Id { get; init; } = string.Empty;
        public double Score { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? PageName { get; init; }
        public string? Snippet { get; init; }
    }
}
