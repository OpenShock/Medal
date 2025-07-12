using OpenShock.Desktop.ModuleBase;
using OpenShock.Desktop.ModuleBase.Navigation;
using OpenShock.Desktop.Modules.Medal;

[assembly:DesktopModule(typeof(MedalModule), "openshock.desktop.modules.medal", "Medal ICYMI")]
namespace OpenShock.Desktop.Modules.Medal;

public sealed class MedalModule : DesktopModuleBase
{
    public override IReadOnlyCollection<NavigationItem> NavigationComponents { get; }
}