using System.Collections.Generic;
using System.Linq;
using ExileCore2;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using ImGuiNET;

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
        if (!Settings.Enable || _session?.Maps == null)
        {
            return;
        }

        if (Settings.ToggleWindowHotkey.PressedOnce())
        {
            _isPanelOpen = !_isPanelOpen;
        }

        if (!_isPanelOpen)
            return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 600), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin($"{Name}", ref _isPanelOpen))
        {
            ImGui.End();
            return;
        }

        if (ImGui.BeginTabBar("MapMetricsTabs"))
        {
            if (ImGui.BeginTabItem("Current Map"))
            {
                RenderCurrentMapStats();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Session Summary"))
            {
                RenderSessionStats();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    private void RenderCurrentMapStats()
    {
        var currentMap = _session.Maps.LastOrDefault();
        if (currentMap == null) return;

        ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), $"Map: {currentMap.AreaName} - T{currentMap.MapTier}");
        ImGui.Text($"Duration: {currentMap.Duration:hh\\:mm\\:ss}");
        ImGui.Text($"IIQ: {currentMap.IncreasedQuantity}% | IIR: {currentMap.IncreasedRarity}%");
        ImGui.Spacing();

        float tableWidth = ImGui.GetContentRegionAvail().X * 0.485f;
        if (ImGui.BeginTable("MonsterCountMainTable", 1, ImGuiTableFlags.Borders, new System.Numerics.Vector2(tableWidth, 0)))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), "Monster Count");

            if (ImGui.BeginTable("MonsterCountDetailsTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Rarity", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Count", ImGuiTableColumnFlags.WidthFixed, 70);
                ImGui.TableHeadersRow();

                int totalMonsters = 0;
                foreach (var (rarity, count) in currentMap.MobsByRarity.OrderBy(x => x.Key))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var color = GetRarityColor(rarity);
                    ImGui.TextColored(color, rarity.ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextColored(color, count.ToString());
                    totalMonsters += count;
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f), "Total");
                ImGui.TableNextColumn();
                ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f), totalMonsters.ToString());

                ImGui.EndTable();
            }
            ImGui.EndTable();
        }

        ImGui.SameLine();
        if (ImGui.BeginTable("ItemDropsMainTable", 1, ImGuiTableFlags.Borders, new System.Numerics.Vector2(tableWidth, 0)))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), "Item Drops");

            if (currentMap.ItemDrops.Count == 0)
            {
                ImGui.TextColored(new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1.0f), "No item drops yet");
            }
            else if (ImGui.BeginTable("ItemDropsDetailsTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Item", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed, 70);
                ImGui.TableHeadersRow();

                var sortedDrops = currentMap.ItemDrops
                    .Select(kvp => (
                        Item: kvp.Key,
                        Count: kvp.Value,
                        Tier: ItemManager.GetItemTier(kvp.Key)))
                    .Where(x => ShouldShowTier(x.Tier))
                    .GroupBy(x => x.Tier)
                    .OrderBy(x => x.Key);

                foreach (var tierGroup in sortedDrops)
                {
                    foreach (var drop in tierGroup.OrderBy(x => x.Item))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextColored(ItemManager.GetTierColor(drop.Tier), drop.Item);
                        ImGui.TableNextColumn();
                        ImGui.TextColored(ItemManager.GetTierColor(drop.Tier), drop.Count.ToString());
                    }
                }

                ImGui.EndTable();
            }
            ImGui.EndTable();
        }
    }
    private bool ShouldShowTier(ItemTier tier)
    {
        return tier switch
        {
            ItemTier.Extreme => Settings.ItemDisplaySettings.ShowExtremeTier,
            ItemTier.High => Settings.ItemDisplaySettings.ShowHighTier,
            ItemTier.Mid => Settings.ItemDisplaySettings.ShowMidTier,
            ItemTier.Low => Settings.ItemDisplaySettings.ShowLowTier,
            _ => true
        };
    }

    private void RenderSessionStats()
    {
        ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), $"Session Started: {_session.StartTime:HH:mm:ss}");
        ImGui.Text($"Total Maps Run: {_session.Maps.Count}");
        ImGui.Spacing();

        var sessionItemDrops = new Dictionary<string, int>();
        foreach (var map in _session.Maps)
        {
            foreach (var (item, count) in map.ItemDrops)
            {
                sessionItemDrops.TryGetValue(item, out int existingCount);
                sessionItemDrops[item] = existingCount + count;
            }
        }

        if (ImGui.BeginTable("SessionSummary", 2, ImGuiTableFlags.None))
        {
            ImGui.TableNextColumn();

            if (ImGui.BeginTable("SessionItemDrops", 1, ImGuiTableFlags.Borders))
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), "Total Item Drops");

                if (sessionItemDrops.Count == 0)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1.0f), "No item drops yet");
                }
                else if (ImGui.BeginTable("ItemTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Item", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed, 70);
                    ImGui.TableHeadersRow();

                    var sortedDrops = sessionItemDrops
                        .Select(kvp => (
                            Item: kvp.Key,
                            Count: kvp.Value,
                            Tier: ItemManager.GetItemTier(kvp.Key)))
                        .Where(x => ShouldShowTier(x.Tier))
                        .GroupBy(x => x.Tier)
                        .OrderBy(x => x.Key);

                    foreach (var tierGroup in sortedDrops)
                    {
                        foreach (var drop in tierGroup.OrderBy(x => x.Item))
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TextColored(ItemManager.GetTierColor(drop.Tier), drop.Item);
                            ImGui.TableNextColumn();
                            ImGui.TextColored(ItemManager.GetTierColor(drop.Tier), drop.Count.ToString());
                        }
                    }

                    ImGui.EndTable();
                }
                ImGui.EndTable();
            }

            ImGui.TableNextColumn();

            if (ImGui.BeginTable("SessionMonsterSummary", 1, ImGuiTableFlags.Borders))
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), "Total Monsters Seen");

                if (ImGui.BeginTable("MonsterTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Rarity", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Count", ImGuiTableColumnFlags.WidthFixed, 70);
                    ImGui.TableHeadersRow();

                    var totalMonsters = new Dictionary<MonsterRarity, int>();
                    foreach (var map in _session.Maps)
                    {
                        foreach (var (rarity, count) in map.MobsByRarity)
                        {
                            totalMonsters.TryGetValue(rarity, out int existingCount);
                            totalMonsters[rarity] = existingCount + count;
                        }
                    }

                    int grandTotal = 0;
                    foreach (var (rarity, count) in totalMonsters.OrderBy(x => x.Key))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        var color = GetRarityColor(rarity);
                        ImGui.TextColored(color, rarity.ToString());
                        ImGui.TableNextColumn();
                        ImGui.TextColored(color, count.ToString());
                        grandTotal += count;
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f), "Total");
                    ImGui.TableNextColumn();
                    ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f), grandTotal.ToString());

                    ImGui.EndTable();
                }
                ImGui.EndTable();
            }

            ImGui.EndTable();
        }

        ImGui.Spacing();
        ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), "Map History");
        if (ImGui.BeginTable("MapHistory", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("Map Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Monsters", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("IIQ", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("IIR", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableHeadersRow();

            foreach (var map in _session.Maps.OrderByDescending(x => x.StartTime))
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
                    RenderMapDetails(map);
                    ImGui.TreePop();
                }
            }
            ImGui.EndTable();
        }
    }

    private void RenderMapDetails(MapRun map)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Indent();

        float tableWidth = ImGui.GetContentRegionAvail().X * 0.485f;

        if (ImGui.BeginTable($"MonsterCountMainTable{map.AreaHash}", 1, ImGuiTableFlags.Borders,
            new System.Numerics.Vector2(tableWidth, 0)))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), "Monster Count");

            if (ImGui.BeginTable($"MonsterCountDetailsTable{map.AreaHash}", 2,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Rarity", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Count", ImGuiTableColumnFlags.WidthFixed, 70);
                ImGui.TableHeadersRow();

                int totalMonsters = 0;
                foreach (var (rarity, count) in map.MobsByRarity.OrderBy(x => x.Key))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var color = GetRarityColor(rarity);
                    ImGui.TextColored(color, rarity.ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextColored(color, count.ToString());
                    totalMonsters += count;
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f), "Total");
                ImGui.TableNextColumn();
                ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f), totalMonsters.ToString());

                ImGui.EndTable();
            }
            ImGui.EndTable();
        }

        ImGui.SameLine();

        if (ImGui.BeginTable($"ItemDropsMainTable{map.AreaHash}", 1, ImGuiTableFlags.Borders,
            new System.Numerics.Vector2(tableWidth, 0)))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f), "Item Drops");

            if (map.ItemDrops.Count == 0)
            {
                ImGui.TextColored(new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1.0f), "No item drops");
            }
            else if (ImGui.BeginTable($"ItemDropsDetailsTable{map.AreaHash}", 2,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Item", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed, 70);
                ImGui.TableHeadersRow();

                var sortedDrops = map.ItemDrops
                    .Select(kvp => (
                        Item: kvp.Key,
                        Count: kvp.Value,
                        Tier: ItemManager.GetItemTier(kvp.Key)))
                    .Where(x => ShouldShowTier(x.Tier))
                    .GroupBy(x => x.Tier)
                    .OrderBy(x => x.Key);

                foreach (var tierGroup in sortedDrops)
                {
                    foreach (var drop in tierGroup.OrderBy(x => x.Item))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextColored(ItemManager.GetTierColor(drop.Tier), drop.Item);
                        ImGui.TableNextColumn();
                        ImGui.TextColored(ItemManager.GetTierColor(drop.Tier), drop.Count.ToString());
                    }
                }

                ImGui.EndTable();
            }
            ImGui.EndTable();
        }

        ImGui.Spacing();
        ImGui.Text($"Start Time: {map.StartTime:HH:mm:ss}");
        if (map.IsCompleted)
        {
            ImGui.SameLine();
            ImGui.Text($"End Time: {map.EndTime:HH:mm:ss}");
        }

        ImGui.Unindent();
    }
    private System.Numerics.Vector4 GetRarityColor(MonsterRarity rarity)
    {
        return rarity switch
        {
            MonsterRarity.White => new System.Numerics.Vector4(0.8f, 0.8f, 0.8f, 1.0f),
            MonsterRarity.Magic => new System.Numerics.Vector4(0.3f, 0.3f, 0.9f, 1.0f),
            MonsterRarity.Rare => new System.Numerics.Vector4(0.9f, 0.9f, 0.1f, 1.0f),
            MonsterRarity.Unique => new System.Numerics.Vector4(0.8f, 0.4f, 0.0f, 1.0f),
            _ => new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f)
        };
    }

    public override void EntityAdded(Entity entity)
    {
        _session.GetCurrentRun(AreaInstance.CurrentHash)?.ProcessEntity(entity);
    }
}