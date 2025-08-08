using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using OpenShock.Desktop.ModuleBase;
using OpenShock.Desktop.ModuleBase.Config;
using OpenShock.Desktop.ModuleBase.Models;
using OpenShock.Desktop.ModuleBase.Navigation;
using OpenShock.Desktop.Modules.Medal;
using OpenShock.Desktop.Modules.Medal.Config;
using OpenShock.Desktop.Modules.Medal.Services;
using OpenShock.Desktop.Modules.Medal.Ui;

[assembly:DesktopModule(typeof(MedalModule), "openshock.desktop.modules.medal", "Medal ICYMI")]
namespace OpenShock.Desktop.Modules.Medal;

public sealed class MedalModule : DesktopModuleBase
{
    private readonly ILogger<MedalModule> _logger;
    private MedalIcymiService? _medalIcymiService;

    public MedalModule(ILogger<MedalModule> logger)
    {
        _logger = logger;
    }
    
    public override IReadOnlyCollection<NavigationItem> NavigationComponents { get; } =
    [
        new()
        {
            Name = "Settings",
            ComponentType = typeof(SettingsTab),
            Icon = IconOneOf.FromSvg(Icons.Material.Filled.Settings)
        }
    ];
        
    public override async Task Setup()
    {
        var config = await ModuleInstanceManager.GetModuleConfig<MedalIcymiConfig>();
        ModuleServiceProvider = BuildServices(config);

        await ModuleInstanceManager.OpenShock.Control.OnLocalControlledShocker.SubscribeAsync(OnLog).ConfigureAwait(false);
        await ModuleInstanceManager.OpenShock.Control.OnRemoteControlledShocker.SubscribeAsync(OnLog).ConfigureAwait(false);
        
        _medalIcymiService = ModuleServiceProvider.GetRequiredService<MedalIcymiService>();
    }

    private async Task OnLog(RemoteControlledShockerArgs remoteControlledShockerArgs)
    {
        if (_medalIcymiService == null)
        {
            _logger.LogError("MedalIcymiService is not initialized. This shouldn't happen unless there is a error in Setup.");
            return;
        }
        
        await _medalIcymiService.TriggerMedalIcymiAction("evt_shockosc_triggered").ConfigureAwait(false);
    }

    private IServiceProvider BuildServices(IModuleConfig<MedalIcymiConfig> config)
    {
        var loggerFactory = ModuleInstanceManager.AppServiceProvider.GetRequiredService<ILoggerFactory>();
        
        var services = new ServiceCollection();

        services.AddSingleton(loggerFactory);
        services.AddLogging();
        services.AddSingleton(config);

        services.AddSingleton(ModuleInstanceManager.OpenShock);

        services.AddSingleton<MedalIcymiService>();
        
        return services.BuildServiceProvider();
    }   
}