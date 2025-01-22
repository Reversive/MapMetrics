using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ExileCore2;
using ExileCore2.Shared.Enums;
using ImGuiNET;
using Newtonsoft.Json;

namespace MapMetrics.UI;

public static class WindowRenderer
{
    public static void RenderCurrentMapWindow(MapRun currentMap, MapMetricsSettings settings)
    {
        if (currentMap == null) return;

        RenderMapHeader(currentMap);
        RenderMapTables(currentMap, settings);
    }

    public static void RenderSessionSummaryWindow(SessionManager sessionManager, MapMetricsSettings settings, string directoryName)
    {
        RenderSessionHeader(sessionManager, sessionManager.CurrentSession, directoryName);
        ImGui.Spacing();
        RenderSessionSummaryTables(sessionManager.CurrentSession, settings);
        RenderMapHistory(sessionManager.CurrentSession, settings);
    }

    private static void RenderMapHeader(MapRun map)
    {
        ImGui.TextColored(ColorHelper.HeaderColor, $"Map: {map.AreaName} - T{map.MapTier}");
        ImGui.Text($"Duration: {map.Duration:hh\\:mm\\:ss}");
        ImGui.Text($"IIQ: {map.IncreasedQuantity}% | IIR: {map.IncreasedRarity}%");
        ImGui.Spacing();
    }

    private static void RenderMapTables(MapRun map, MapMetricsSettings settings)
    {
        float tableWidth = ImGui.GetContentRegionAvail().X * 0.485f;

        if (ImGui.BeginTable("MonsterCountMainTable", 1, ImGuiTableFlags.Borders, new Vector2(tableWidth, 0)))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(ColorHelper.HeaderColor, "Monster Count");
            TableRenderer.RenderMonsterTable(map.MobsByRarity, "Monster Count", new Vector2(tableWidth, 0));
            ImGui.EndTable();
        }

        ImGui.SameLine();

        if (ImGui.BeginTable("ItemDropsMainTable", 1, ImGuiTableFlags.Borders, new Vector2(tableWidth, 0)))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(ColorHelper.HeaderColor, "Item Drops");

            if (map.ItemDrops.Count == 0)
                ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), "No item drops yet");
            else
                TableRenderer.RenderItemDropsTable(map.ItemDrops, settings);

            ImGui.EndTable();
        }
    }

    private static void RenderSessionHeader(SessionManager sessionManager, Session session, string directoryName)
    {
        ImGui.TextColored(ColorHelper.HeaderColor, $"Session Started: {session.StartTime:HH:mm:ss}");
        ImGui.Text($"Total Maps Run: {session.Maps.Count}");
        
        ImGui.SameLine(ImGui.GetWindowWidth() - 150);
        if (ImGui.Button("Stop Session"))
        {
            sessionManager.StopCurrentSession();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Stop current session and start a new one");
        }
        ImGui.Spacing();
    }

    private static void RenderSessionSummaryTables(Session session, MapMetricsSettings settings)
    {
        if (!ImGui.BeginTable("SessionSummary", 2, ImGuiTableFlags.None))
            return;

        ImGui.TableNextColumn();
        RenderSessionItemDrops(session, settings);

        ImGui.TableNextColumn();
        RenderSessionMonsterSummary(session);

        ImGui.EndTable();
    }

    private static void RenderSessionItemDrops(Session session, MapMetricsSettings settings)
    {
        var sessionItemDrops = GetSessionItemDrops(session);

        if (!ImGui.BeginTable("SessionItemDrops", 1, ImGuiTableFlags.Borders))
            return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextColored(ColorHelper.HeaderColor, "Total Item Drops");

        if (sessionItemDrops.Count == 0)
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), "No item drops yet");
        else
            TableRenderer.RenderItemDropsTable(sessionItemDrops, settings);

        ImGui.EndTable();
    }

    private static void RenderSessionMonsterSummary(Session session)
    {
        if (!ImGui.BeginTable("SessionMonsterSummary", 1, ImGuiTableFlags.Borders))
            return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextColored(ColorHelper.HeaderColor, "Total Monsters Seen");

        var totalMonsters = GetSessionMonsterCounts(session);
        TableRenderer.RenderMonsterTable(totalMonsters, "Monster Count", Vector2.Zero);

        ImGui.EndTable();
    }

    private static void RenderMapHistory(Session session, MapMetricsSettings settings)
    {
        ImGui.Spacing();
        ImGui.TextColored(ColorHelper.HeaderColor, "Map History");

        if (!ImGui.BeginTable("MapHistory", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
            return;

        RenderMapHistoryHeaders();
        RenderMapHistoryRows(session, settings);
        ImGui.EndTable();
    }

    private static void RenderMapHistoryHeaders()
    {
        ImGui.TableSetupColumn("Map Name", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("Monsters", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("IIQ", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("IIR", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableHeadersRow();
    }

    private static void RenderMapHistoryRows(Session session, MapMetricsSettings settings)
    {
        foreach (var map in session.Maps.OrderByDescending(x => x.StartTime))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            var isExpanded = ImGui.TreeNodeEx($"{map.AreaName} - T{map.MapTier}", ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TableNextColumn();
            ImGui.Text(map.Duration.ToString(@"mm\:ss"));
            ImGui.TableNextColumn();
            ImGui.Text(map.MobsByRarity.Values.Sum().ToString());
            ImGui.TableNextColumn();
            ImGui.Text(map.IncreasedQuantity.ToString());
            ImGui.TableNextColumn();
            ImGui.Text(map.IncreasedRarity.ToString());

            if (isExpanded)
            {
                RenderMapHistoryDetails(map, settings);
                ImGui.TreePop();
            }
        }
    }

    private static void RenderMapHistoryDetails(MapRun map, MapMetricsSettings settings)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Indent();

        float tableWidth = ImGui.GetContentRegionAvail().X * 0.485f;
        RenderMapTables(map, settings);

        ImGui.Spacing();
        ImGui.Text($"Start Time: {map.StartTime:HH:mm:ss}");
        if (map.IsCompleted)
        {
            ImGui.SameLine();
            ImGui.Text($"End Time: {map.EndTime:HH:mm:ss}");
        }

        ImGui.Unindent();
    }

    private static Dictionary<string, int> GetSessionItemDrops(Session session)
    {
        var sessionItemDrops = new Dictionary<string, int>();
        foreach (var map in session.Maps)
        {
            foreach (var (item, count) in map.ItemDrops)
            {
                sessionItemDrops.TryGetValue(item, out int existingCount);
                sessionItemDrops[item] = existingCount + count;
            }
        }
        return sessionItemDrops;
    }

    private static Dictionary<MonsterRarity, int> GetSessionMonsterCounts(Session session)
    {
        var totalMonsters = new Dictionary<MonsterRarity, int>();
        foreach (var map in session.Maps)
        {
            foreach (var (rarity, count) in map.MobsByRarity)
            {
                totalMonsters.TryGetValue(rarity, out int existingCount);
                totalMonsters[rarity] = existingCount + count;
            }
        }
        return totalMonsters;
    }

    public static void RenderSessionHistoryWindow(IReadOnlyList<Session> sessions, MapMetricsSettings settings, string directoryName)
    {
        if (ImGui.Button("Export All Sessions"))
        {
            ExportAllSessions(sessions, directoryName);
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Export all sessions to a single file");
        }

        ImGui.SameLine();
        ImGui.TextColored(ColorHelper.HeaderColor, $"Total Sessions: {sessions.Count}");
        ImGui.Spacing();
        if (!ImGui.BeginTable("SessionHistory", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
            return;

        ImGui.TableSetupColumn("Start Time", ImGuiTableColumnFlags.WidthFixed, 150);
        ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("Maps Run", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("Details", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        foreach (var session in sessions.OrderByDescending(x => x.StartTime))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            var isExpanded = ImGui.TreeNodeEx($"{session.StartTime:yyyy-MM-dd HH:mm:ss}", ImGuiTreeNodeFlags.SpanFullWidth);
            ImGui.TableNextColumn();
            ImGui.Text(session.Duration.ToString(@"hh\:mm\:ss"));
            ImGui.TableNextColumn();
            ImGui.Text(session.Maps.Count.ToString());
            ImGui.TableNextColumn();
            if (ImGui.Button($"Export##{session.StartTime.Ticks}"))
            {
                ExportSession(session, directoryName);
            }

            if (isExpanded)
            {
                RenderSessionHistoryDetails(session, settings);
                ImGui.TreePop();
            }
        }

        ImGui.EndTable();
    }

    private static void RenderSessionHistoryDetails(Session session, MapMetricsSettings settings)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Indent();

        RenderSessionSummaryTables(session, settings);
        RenderMapHistory(session, settings);

        ImGui.Unindent();
    }

    private static void ExportSession(Session session, string directoryName)
    {
        var sessionExport = SessionExport.FromSession(session);
        var fileName = $"map_metrics_session_{session.StartTime:yyyy-MM-dd_HH-mm-ss}.json";
        var filePath = Path.Combine(directoryName, fileName);

        try
        {
            var jsonString = JsonConvert.SerializeObject(sessionExport, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
            DebugWindow.LogMsg($"Session data exported to {fileName}");
        }
        catch (Exception e)
        {
            DebugWindow.LogError($"Failed to export session data: {e.Message}");
        }
    }

    private static void ExportAllSessions(IReadOnlyList<Session> sessions, string directoryName)
    {
        try
        {
            var multiSessionExport = MultiSessionExport.FromSessions(sessions);
            var fileName = $"map_metrics_full_session_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
            var filePath = Path.Combine(directoryName, fileName);

            var jsonString = JsonConvert.SerializeObject(multiSessionExport, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
            DebugWindow.LogMsg($"All sessions exported to {fileName}");
        }
        catch (Exception e)
        {
            DebugWindow.LogError($"Failed to export all sessions: {e.Message}");
        }
    }
}