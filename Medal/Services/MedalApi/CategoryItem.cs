namespace OpenShock.Desktop.Modules.Medal.Services.MedalApi;

public sealed class CategoryItem
{
    public required string CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public required string Slug { get; set; }
    public required Uri CategoryThumbnail { get; set; }
}