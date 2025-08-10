namespace OpenShock.Desktop.Modules.Medal.Services.MedalApi;

public interface IMedalApiService
{
    public Task<IReadOnlyList<CategoryItem>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}