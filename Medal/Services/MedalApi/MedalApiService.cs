using System.Text.Json;

namespace OpenShock.Desktop.Modules.Medal.Services.MedalApi;

public sealed class MedalApiService : IMedalApiService
{
    private readonly HttpClient _httpClient;

    public MedalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IReadOnlyList<CategoryItem>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categoriesRequest = await _httpClient.GetAsync("https://api-v2.medal.tv/categories", cancellationToken);
        if (!categoriesRequest.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch categories: {categoriesRequest.ReasonPhrase}");
        }

        var categoryItems = await JsonSerializer.DeserializeAsync<IReadOnlyList<CategoryItem>>(await categoriesRequest.Content
            .ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        if(categoryItems == null)
        {
            throw new InvalidOperationException("Failed to deserialize category items.");
        }
        
        return categoryItems;
    }
}