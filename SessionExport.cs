using System.Collections.Generic;
using System.Linq;
using System;

namespace MapMetrics;

public class SessionExport
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public List<MapRunExport> Maps { get; set; }

    public class MapRunExport
    {
        public string AreaName { get; set; }
        public uint AreaHash { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Dictionary<string, int> ItemDrops { get; set; }
        public Dictionary<string, int> MobsByRarity { get; set; }
        public int IncreasedQuantity { get; set; }
        public int IncreasedRarity { get; set; }
        public int MapTier { get; set; }
    }

    public static SessionExport FromSession(Session session)
    {
        return new SessionExport
        {
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Duration = session.Duration,
            Maps = session.Maps.Select(map => new MapRunExport
            {
                AreaName = map.AreaName,
                AreaHash = map.AreaHash,
                StartTime = map.StartTime,
                EndTime = map.IsCompleted ? map.EndTime : null,
                ItemDrops = map.ItemDrops,
                MobsByRarity = map.MobsByRarity.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                ),
                IncreasedQuantity = map.IncreasedQuantity,
                IncreasedRarity = map.IncreasedRarity,
                MapTier = map.MapTier
            }).ToList()
        };
    }
}