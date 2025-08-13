using System.Text.Json.Serialization;

namespace OpenShock.Desktop.Modules.Medal.Services.MedalApi;

public sealed class RegisterRequest
{
    [JsonPropertyName("appName")]
    public required string Name { get; set; }
    
    [JsonPropertyName("appUrl")]
    public required string Url { get; set; }
    
    [JsonPropertyName("categoryId")]
    public required string CategoryId { get; set; }
    
}