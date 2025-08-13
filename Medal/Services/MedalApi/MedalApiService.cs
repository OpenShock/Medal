using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace OpenShock.Desktop.Modules.Medal.Services.MedalApi;

public sealed class MedalApiService : IMedalApiService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public MedalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryItem>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categoriesRequest = await _httpClient.GetAsync("https://api-v2.medal.tv/categories", cancellationToken);
        if (!categoriesRequest.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch categories: {categoriesRequest.ReasonPhrase}");
        }

        var categoryItems = await JsonSerializer.DeserializeAsync<IReadOnlyList<CategoryItem>>(await categoriesRequest.Content
            .ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken, options: JsonSerializerOptions);

        if(categoryItems == null)
        {
            throw new InvalidOperationException("Failed to deserialize category items.");
        }
        
        return categoryItems;
    }

    /// <inheritdoc />
    public async Task<string> RegisterAppAsync(string name, string url, string categoryId, CancellationToken cancellationToken = default)
    {
        var requestBody = new RegisterRequest
        {
            Name = name,
            Url = url,
            CategoryId = categoryId
        };
        
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        
        var registerRequest = await _httpClient.PostAsync(
            "https://register-thirdpartyapp.medal.workers.dev/", content, cancellationToken);

        if (!registerRequest.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to register app: {registerRequest.ReasonPhrase}");
        }
        
        var responseContent = await registerRequest.Content.ReadAsStreamAsync(cancellationToken);
        var response = await JsonSerializer.DeserializeAsync<RegisterResponse>(responseContent, cancellationToken: cancellationToken, options: JsonSerializerOptions);
        if (response == null)
        {
            throw new InvalidOperationException("Failed to deserialize register response.");
        }
        
        if (string.IsNullOrEmpty(response.ApiKey))
        {
            throw new InvalidOperationException("Public key is missing in the response.");
        }

        return response.ApiKey;
    }
}