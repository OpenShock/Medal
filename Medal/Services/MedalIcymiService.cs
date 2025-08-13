using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenShock.Desktop.ModuleBase.Config;
using OpenShock.Desktop.ModuleBase.Models;
using OpenShock.Desktop.Modules.Medal.Config;

namespace OpenShock.Desktop.Modules.Medal.Services;

public sealed class MedalIcymiService
{
    private readonly ILogger<MedalIcymiService> _logger;
    private readonly IModuleConfig<MedalIcymiConfig> _moduleConfig;
    private static readonly HttpClient HttpClient = new();
    private const string BaseUrl = "http://localhost:12665/api/v1";

    public MedalIcymiService(ILogger<MedalIcymiService> logger, IModuleConfig<MedalIcymiConfig> moduleConfig)
    {
        _logger = logger;
        _moduleConfig = moduleConfig;
    }

    public void ShockerTriggered(RemoteControlledShockerArgs args, bool remote)
    {
        if (remote && !_moduleConfig.Config.Remote)
        {
            _logger.LogDebug("Remote control is disabled in configuration. Ignoring remote trigger.");
            return;
        }

        if (!remote && !_moduleConfig.Config.Local)
        {
            _logger.LogDebug("Local control is disabled in configuration. Ignoring local trigger.");
            return;
        }

        if (!args.Logs.Any(x => _moduleConfig.Config.EnabledControlTypes.Contains(x.Type)))
        {
            _logger.LogDebug("No logs match the enabled control types in configuration. Ignoring trigger.");
            return;
        }
        
        var longestCommand = args.Logs.Max(x => x.Duration);
        
        var delay = _moduleConfig.Config.IncludeDurationInDelay
            ? _moduleConfig.Config.TriggerDelay + TimeSpan.FromMilliseconds(longestCommand)
            : _moduleConfig.Config.TriggerDelay;

        if (!_moduleConfig.Config.GameKeys.TryGetValue(_moduleConfig.Config.Game, out var gameKeyValue))
        {
            _logger.LogWarning("Game key for {game} not found in configuration. Cannot trigger Medal ICYMI.", _moduleConfig.Config.Game);
            return;
        }
        
        _logger.LogInformation("Medal ICYMI Triggered: {triggerAction} with delay {delay} for {game}",
            _moduleConfig.Config.TriggerAction, delay, _moduleConfig.Config.Game);
        
        Task.Run(() => TriggerMedalInternal(delay, _moduleConfig.Config.Game, gameKeyValue)).ContinueWith(task =>
        {
            if(task.IsFaulted) 
            {
                _logger.LogError("Error triggering Medal ICYMI: {exception}", task.Exception);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task TriggerMedalInternal(TimeSpan delay, string game, string gamePublicKey)
    {
        if(delay != TimeSpan.Zero) await Task.Delay(delay);
        
        var eventPayload = new
        {
            eventId = "openshock_desktop_",
            eventName = _moduleConfig.Config.Name,
            
            contextTags = new
            {
                location = game,
                description = _moduleConfig.Config.Description
            },
            triggerActions = new[]
            {
                _moduleConfig.Config.TriggerAction.ToString()
            },
            
            clipOptions = new
            {
                duration = _moduleConfig.Config.ClipDuration,
                alertType = _moduleConfig.Config.AlertType.ToString(),
            }
        };

        var jsonPayload = JsonSerializer.Serialize(eventPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/event/invoke");
            request.Content = content;
            request.Headers.Add("publicKey", gamePublicKey);
            var response = await HttpClient.SendAsync(request);

            _logger.LogInformation("{triggerAction} triggered.", _moduleConfig.Config.TriggerAction);
                
            var responseContent = await response.Content.ReadAsStringAsync();
            HandleApiResponse((int)response.StatusCode, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while creating Medal {triggerAction}: {exception}", _moduleConfig.Config.TriggerAction, ex);
        }
    }

    private void HandleApiResponse(int statusCode, string responseContent)
    {
        switch (statusCode)
        {
            case 400 when responseContent.Contains("INVALID_MODEL"):
                _logger.LogError("Invalid model: The request body does not match the expected model structure.");
                break;

            case 400 when responseContent.Contains("INVALID_EVENT"):
                _logger.LogError("Invalid event: The provided game event details are invalid.");
                break;

            case 400 when responseContent.Contains("MISSING_PUBLIC_KEY"):
                _logger.LogError("Missing public key: The publicKey header is missing from the request.");
                break;

            case 400 when responseContent.Contains("INVALID_APP_DATA"):
                _logger.LogError("Invalid app data: Failed to retrieve app data associated with the provided public key.");
                break;

            case 200 when responseContent.Contains("INACTIVE_GAME"):
                _logger.LogWarning("Inactive game: The event was received but not processed because the categoryId does not match the active game.");
                break;

            case 200 when responseContent.Contains("DISABLED_EVENT"):
                _logger.LogWarning("Disabled event: The event was received but not processed because it is disabled in the user’s ICYMI settings.");
                break;

            case 200 when responseContent.Contains("success"):
                _logger.LogDebug("Event received and processed successfully");
                break;
            
            case 500:
                _logger.LogError("Internal server error: An unexpected error occurred while processing the request.");
                break;

            default:
                _logger.LogWarning("Unexpected response: {statusCode} - {response}", statusCode, responseContent);
                break;
        }
    }
}