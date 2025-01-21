using System.Linq;
using System.Numerics;
using ExileCore2;
using ExileCore2.PoEMemory.MemoryObjects;
using ImGuiNET;
using MapMetrics.UI;

namespace MapMetrics;

public class MapMetrics : BaseSettingsPlugin<MapMetricsSettings>
{
    private Session _session;
    private bool _isPanelOpen = false;

    public override bool Initialise()
    {
        _session = new Session(GameController);
        Input.RegisterKey(Settings.ToggleWindowHotkey);
        Settings.ToggleWindowHotkey.OnValueChanged += () => Input.RegisterKey(Settings.ToggleWindowHotkey);
        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
       if(area.IsHideout || area.IsPeaceful || area.IsTown)
       {
            _session.StopRun();
            return;
       }
        
        if (_session.Exists(area.Hash))
        {
            DebugWindow.LogMsg($"Area {area.Name} is the same as last time");
            _session.ResumeRun(area.Hash);
            return;
        }

        DebugWindow.LogMsg($"Entering area {area.Name} for the first time");
        _session.StartRun(area.Name, area.Hash);
    }

    public override void Tick()
    {
    }

    public override void Render()
    {
        if(!ShouldRender())
            return;

        if (Settings.ToggleWindowHotkey.PressedOnce())
        {
            _isPanelOpen = !_isPanelOpen;
        }

        if (!_isPanelOpen)
            return;
        
        RenderMainWindow();
    }

 
    public override void EntityAdded(Entity entity)
    {
        _session.GetCurrentRun(AreaInstance.CurrentHash)?.ProcessEntity(entity);
    }

    private bool ShouldRender() =>
        Settings.Enable && _session?.Maps != null;

    private void RenderMainWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin($"{Name}", ref _isPanelOpen))
        {
            ImGui.End();
            return;
        }

        RenderTabBar();
        ImGui.End();
    }

    private void RenderTabBar()
    {
        if (!ImGui.BeginTabBar("MapMetricsTabs"))
            return;

        if (ImGui.BeginTabItem("Current Map"))
        {
            WindowRenderer.RenderCurrentMapWindow(_session.Maps.LastOrDefault(), Settings);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Session Summary"))
        {
            WindowRenderer.RenderSessionSummaryWindow(_session, Settings, DirectoryFullName);
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }
}