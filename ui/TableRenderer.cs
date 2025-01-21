using System.Collections.Generic;
using System.Linq;
using ExileCore2.Shared.Enums;
using ImGuiNET;
using System.Numerics;

namespace MapMetrics.UI;

public static class TableRenderer
{
    public static void RenderMonsterTable(Dictionary<MonsterRarity, int> monsters, string title, Vector2 size)
    {
        if (!ImGui.BeginTable("MonsterCountDetailsTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            return;

        ImGui.TableSetupColumn("Rarity", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Count", ImGuiTableColumnFlags.WidthFixed, 70);
        ImGui.TableHeadersRow();

        int totalMonsters = 0;
        foreach (var (rarity, count) in monsters.OrderBy(x => x.Key))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            var color = ColorHelper.GetRarityColor(rarity);
            ImGui.TextColored(color, rarity.ToString());
            ImGui.TableNextColumn();
            ImGui.TextColored(color, count.ToString());
            totalMonsters += count;
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextColored(ColorHelper.DefaultColor, "Total");
        ImGui.TableNextColumn();
        ImGui.TextColored(ColorHelper.DefaultColor, totalMonsters.ToString());

        ImGui.EndTable();
    }

    public static void RenderItemDropsTable(Dictionary<string, int> items, MapMetricsSettings settings)
    {
        if (!ImGui.BeginTable("ItemDropsDetailsTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            return;

        ImGui.TableSetupColumn("Item", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed, 70);
        ImGui.TableHeadersRow();

        var sortedDrops = items
            .Select(kvp => (
                Item: kvp.Key,
                Count: kvp.Value,
                Tier: ItemManager.GetItemTier(kvp.Key)))
            .Where(x => ShouldShowTier(x.Tier, settings))
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

    private static bool ShouldShowTier(ItemTier tier, MapMetricsSettings settings) =>
        tier switch
        {
            ItemTier.Extreme => settings.ItemDisplaySettings.ShowExtremeTier,
            ItemTier.High => settings.ItemDisplaySettings.ShowHighTier,
            ItemTier.Mid => settings.ItemDisplaySettings.ShowMidTier,
            ItemTier.Low => settings.ItemDisplaySettings.ShowLowTier,
            _ => true
        };
}