using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using ExileCore2;
using System;
using System.Collections.Generic;

namespace MapMetrics;

public class MapRun
{
    private bool _isCompleted;
    private GameController _gameController;

    public MapRun(string areaName, uint areaHash, GameController gameController)
    {
        DebugWindow.LogMsg($"Starting run in {areaName}");
        AreaName = areaName;
        AreaHash = areaHash;
        StartTime = DateTime.Now;
        ItemDrops = [];
        MobsByRarity = [];
        _isCompleted = false;
        _gameController = gameController;
        var mapStats = gameController.IngameState.Data.MapStats;
        IncreasedQuantity = mapStats?.GetValueOrDefault(GameStat.MapItemDropQuantityPct) ?? 0;
        IncreasedRarity = mapStats?.GetValueOrDefault(GameStat.MapItemDropRarityPct) ?? 0;
        MapTier = mapStats?.GetValueOrDefault(GameStat.MapTier) ?? 0;

        foreach (MonsterRarity rarity in Enum.GetValues(typeof(MonsterRarity)))
        {
            if (rarity == MonsterRarity.Error) continue;
            MobsByRarity[rarity] = 0;
        }
    }

    public void ProcessEntity(Entity entity)
    {
        if (entity.HasComponent<Monster>())
        {
            TrackMonster(entity);
        }
        else if (entity.HasComponent<WorldItem>())
        {
            TrackItem(entity);
        }
    }

    private void TrackMonster(Entity monster)
    {
        MonsterRarity rarity = monster.GetComponent<ObjectMagicProperties>()?.Rarity ?? MonsterRarity.White;
        MobsByRarity.TryGetValue(rarity, out int count);
        MobsByRarity[rarity] = count + 1;
    }

    private void TrackItem(Entity item)
    {
        var itemEntity = item.GetComponent<WorldItem>().ItemEntity;
        var baseItemType = _gameController.Files.BaseItemTypes.Translate(itemEntity.Path);
        if (baseItemType == null) return;
        if (baseItemType.ClassName.Contains("StackableCurrency"))
        {
            var amount = itemEntity.GetComponent<Stack>().Size;
            ItemDrops.TryGetValue(baseItemType.BaseName, out int count);
            ItemDrops[baseItemType.BaseName] = count + amount;
        }
        else if (baseItemType.ClassName.Contains("TowerAugmentation")
                || baseItemType.ClassName.Contains("PinnacleKey")
                || baseItemType.ClassName.Contains("UltimatumKey")
                || baseItemType.ClassName.Contains("ExpeditionLogbook"))
        {
            ItemDrops.TryGetValue(baseItemType.BaseName, out int count);
            ItemDrops[baseItemType.BaseName] = count + 1;
        }
    }

    public void End()
    {
        if (_isCompleted)
        {
            return;
        }

        EndTime = DateTime.Now;
        _isCompleted = true;
    }

    public void Resume()
    {
        if (!_isCompleted)
        {
            return;
        }
        _isCompleted = false;
        StartTime = DateTime.Now.Subtract(EndTime.Subtract(StartTime));

    }

    public string AreaName { get; }
    public uint AreaHash { get; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; private set; }
    public TimeSpan Duration => _isCompleted ?
        EndTime.Subtract(StartTime) :
        DateTime.Now.Subtract(StartTime);
    public bool IsCompleted => _isCompleted;
    public Dictionary<string, int> ItemDrops { get; }
    public Dictionary<MonsterRarity, int> MobsByRarity { get; }
    public int IncreasedQuantity { get; }
    public int IncreasedRarity { get; }
    public int MapTier { get; }
}

