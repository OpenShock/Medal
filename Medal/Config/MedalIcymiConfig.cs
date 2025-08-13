// ReSharper disable InconsistentNaming

using OpenShock.Desktop.ModuleBase.Models;

namespace OpenShock.Desktop.Modules.Medal.Config;

public sealed class MedalIcymiConfig
{
    public bool Enabled { get; set; } = false;
    public string Name { get; set; } = "OpenShock Desktop";
    public string Description { get; set; } = "OpenShock activated.";
    public int ClipDuration { get; set; } = 30;
    public IcymiAlertType AlertType { get; set; } = IcymiAlertType.Default;
    public IcymiTriggerAction TriggerAction { get; set; } = IcymiTriggerAction.SaveClip;
    public string Game { get; set; } = "VRChat";
    public IDictionary<string, string> GameKeys { get; set; } = new Dictionary<string, string>
    {
        { "VRChat", "pub_x4PTxSGVk6sl8BYg5EB5qsn8QIVz4kRi" },
        { "ChilloutVR", "pub_LRG3bA6XjoVSkSU4JuXmL51tJdGJWdVQ" }
    };
    public TimeSpan TriggerDelay { get; set; } = TimeSpan.FromSeconds(5);
    public bool IncludeDurationInDelay { get; set; } = true;
    public IList<ControlType> EnabledControlTypes { get; set; } = new List<ControlType>
    {
        ControlType.Shock
    };
    public bool Remote { get; set; } = true;
    public bool Local { get; set; } = true;
    
}

public enum IcymiTriggerAction
{
    SaveClip,
    SaveScreenshot
}

public enum IcymiAlertType
{
    Default,
    Disabled,
    SoundOnly,
    OverlayOnly
}