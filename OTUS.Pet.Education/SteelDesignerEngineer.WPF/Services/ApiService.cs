using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using SteelDesignerEngineer.WPF.Models;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Реализация сервиса для работы с API портала
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<HtmlPageModel>> GetPagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/PageContent");
            response.EnsureSuccessStatusCode();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiListResponse>(_jsonOptions);
            if (apiResponse?.Success != true || apiResponse.Items == null)
            {
                return new List<HtmlPageModel>();
            }
            
            return apiResponse.Items;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка получения списка страниц: {ex.Message}", ex);
        }
    }

    public async Task<HtmlPageModel?> GetPageByNameAsync(string pageName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/PageContent/by-name/{pageName}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiItemResponse>(_jsonOptions);
            return apiResponse?.Success == true ? apiResponse.Item : null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка получения страницы {pageName}: {ex.Message}", ex);
        }
    }

    public async Task<HtmlPageModel> CreatePageAsync(HtmlPageModel page)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/PageContent", page, _jsonOptions);
            response.EnsureSuccessStatusCode();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiItemResponse>(_jsonOptions);
            if (apiResponse?.Success != true || apiResponse.Item == null)
            {
                throw new Exception("Не удалось создать страницу");
            }
            
            return apiResponse.Item;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка создания страницы: {ex.Message}", ex);
        }
    }

    public async Task<HtmlPageModel> UpdatePageAsync(HtmlPageModel page)
    {
        try
        {
            // Use POST to create/update since there's no PUT endpoint
            var response = await _httpClient.PostAsJsonAsync("/api/PageContent", page, _jsonOptions);
            response.EnsureSuccessStatusCode();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiItemResponse>(_jsonOptions);
            if (apiResponse?.Success != true || apiResponse.Item == null)
            {
                throw new Exception("Не удалось обновить страницу");
            }
            
            return apiResponse.Item;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка обновления страницы: {ex.Message}", ex);
        }
    }

    public async Task DeletePageAsync(string pageName)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/PageContent/by-name/{pageName}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка удаления страницы: {ex.Message}", ex);
        }
    }

    public async Task<bool> CheckApiHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/swagger/index.html");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckApiAvailabilityAsync()
    {
        return await CheckApiHealthAsync();
    }

    public string GetBaseUrl()
    {
        return _httpClient.BaseAddress?.ToString() ?? "Unknown";
    }

    // Helper classes for API responses
    private class ApiListResponse
    {
        public bool Success { get; set; }
        public List<HtmlPageModel>? Items { get; set; }
    }

    private class ApiItemResponse
    {
        public bool Success { get; set; }
        public HtmlPageModel? Item { get; set; }
    }
}
