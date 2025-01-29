using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;

namespace MapMetrics;

public class MapMetricsSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu("Show/Hide Window Hotkey")]
    public HotkeyNode ToggleWindowHotkey { get; set; } = new HotkeyNode(System.Windows.Forms.Keys.F7);

    [Menu("Currency Display Settings", "Toggle which currency tiers to display")]
    public TierSettings ItemDisplaySettings { get; set; } = new TierSettings();
    [Menu("Auto Save Session", "Automatically save current session every 5 seconds")]
    public ToggleNode AutoSaveSession { get; set; } = new ToggleNode(true);
}

[Submenu(CollapsedByDefault = false)]
public class TierSettings
{
    [Menu("Show Extreme Tier")]
    public ToggleNode ShowExtremeTier { get; set; } = new ToggleNode(true);

    [Menu("Show High Tier")]
    public ToggleNode ShowHighTier { get; set; } = new ToggleNode(true);

    [Menu("Show Mid Tier")]
    public ToggleNode ShowMidTier { get; set; } = new ToggleNode(true);

    [Menu("Show Low Tier")]
    public ToggleNode ShowLowTier { get; set; } = new ToggleNode(true);
}