namespace OpenShock.Desktop.Modules.Medal.Services.MedalApi;

public interface IMedalApiService
{
    /// <summary>
    /// Get all game categories from the Medal API.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IReadOnlyList<CategoryItem>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Create a new third-party app on the Medal API.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="url"></param>
    /// <param name="categoryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<string> RegisterAppAsync(string name, string url, string categoryId, CancellationToken cancellationToken = default);
}