using System;
using System.IO;
using System.Linq;
using System.Numerics;
using ExileCore2;
using ExileCore2.PoEMemory.MemoryObjects;
using ImGuiNET;
using MapMetrics.UI;
using Newtonsoft.Json;

namespace MapMetrics;

public class MapMetrics : BaseSettingsPlugin<MapMetricsSettings>
{
    private SessionManager _sessionManager;
    private bool _isPanelOpen = false;
    private DateTime _lastAutoSave = DateTime.Now;
    private string _currentSessionFile;

    public override bool Initialise()
    {
        _sessionManager = new SessionManager(GameController, DirectoryFullName);
        Input.RegisterKey(Settings.ToggleWindowHotkey);
        Settings.ToggleWindowHotkey.OnValueChanged += () => Input.RegisterKey(Settings.ToggleWindowHotkey);
        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
       if(area.IsHideout || area.IsPeaceful || area.IsTown)
       {
            _sessionManager.CurrentSession.StopRun();
            return;
       }
        
        if (_sessionManager.CurrentSession.Exists(area.Hash))
        {
            DebugWindow.LogMsg($"Area {area.Name} is the same as last time");
            _sessionManager.CurrentSession.ResumeRun(area.Hash);
            return;
        }

        DebugWindow.LogMsg($"Entering area {area.Name} for the first time");
        _currentSessionFile = null;
        _sessionManager.CurrentSession.StartRun(area.Name, area.Hash);
    }

    public override void Tick()
    {
        HandleAutoSave();
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
        _sessionManager.CurrentSession.GetCurrentRun(AreaInstance.CurrentHash)?.ProcessEntity(entity);
    }

    private bool ShouldRender() =>
        Settings.Enable && _sessionManager.CurrentSession?.Maps != null;

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
            WindowRenderer.RenderCurrentMapWindow(_sessionManager.CurrentSession.Maps.LastOrDefault(), Settings);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Session Summary"))
        {
            WindowRenderer.RenderSessionSummaryWindow(_sessionManager, Settings, DirectoryFullName);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Session History"))
        {
            WindowRenderer.RenderSessionHistoryWindow(_sessionManager.CompletedSessions, Settings, DirectoryFullName);
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private void HandleAutoSave()
    {
        if (!Settings.AutoSaveSession || _sessionManager.CurrentSession == null)
            return;

        var now = DateTime.Now;
        if ((now - _lastAutoSave).TotalSeconds >= 5)
        {
            AutoSaveCurrentSession();
            _lastAutoSave = now;
        }
    }

    private void AutoSaveCurrentSession()
    {
        try
        {
            var filePath = _sessionManager.GetCurrentSessionFilePath(DirectoryFullName);
            var sessionExport = SessionExport.FromSession(_sessionManager.CurrentSession);
            sessionExport.EndTime = DateTime.Now;
            var jsonString = JsonConvert.SerializeObject(sessionExport, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception e)
        {
            DebugWindow.LogError($"Failed to auto-save session: {e.Message}");
        }
    }


    public SessionManager SessionManager => _sessionManager;
}