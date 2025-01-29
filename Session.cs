using ExileCore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMetrics;

public class Session(GameController gameController)
{
    private List<MapRun> _maps = [];
    private GameController _gameController = gameController;
    public DateTime StartTime { get; set; } = DateTime.Now;

    public void StartRun(string areaName, uint areaHash)
    {
        _maps.Add(new MapRun(areaName, areaHash, _gameController));
    }

    public bool Exists(uint areaHash)
    {
        return _maps.Exists(map => map.AreaHash == areaHash);
    }

    public void ResumeRun(uint areaHash)
    {
        MapRun mapRun = _maps.Find(map => map.AreaHash == areaHash);
        DebugWindow.LogMsg($"Resuming run {mapRun?.AreaName}");
        mapRun?.Resume();
    }

    public void StopRun()
    {
        if (_maps.Count == 0)
        {
            DebugWindow.LogMsg("No run to stop");
            return;
        }
        DebugWindow.LogMsg($"Stopping run {_maps[^1]?.AreaName}");
        _maps[^1]?.End();
    }

    public MapRun GetCurrentRun(uint areaHash)
    {
        if (_maps.Count == 0)
        {
            return null;
        }
        return _maps.Find(map => map.AreaHash == areaHash);
    }

    public List<MapRun> Maps { get => _maps; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => (EndTime ?? DateTime.Now).Subtract(StartTime);
}
