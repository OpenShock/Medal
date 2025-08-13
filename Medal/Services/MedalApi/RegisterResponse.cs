using System.Text.Json.Serialization;

namespace OpenShock.Desktop.Modules.Medal.Services.MedalApi;

public sealed class RegisterResponse
{
    [JsonPropertyName("genApiKey")]
    public required string ApiKey { get; set; }
}