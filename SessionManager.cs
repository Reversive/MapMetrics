using ExileCore2;
using ExileCore2.Shared.Enums;
using MapMetrics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMetrics;
public class SessionManager
{
    private List<Session> _completedSessions = new();
    private Session _currentSession;
    private GameController _gameController;
    private string _currentSessionFilePath;

    public SessionManager(GameController gameController, String directory)
    {
        _gameController = gameController;
        LoadSavedSessions(directory);
        StartNewSession();
    }

    public void StartNewSession()
    {
        _currentSession = new Session(_gameController);
        _currentSessionFilePath = null;
    }

    public void StopCurrentSession()
    {
        if (_currentSession != null)
        {
            _currentSession.EndTime = DateTime.Now;
            _completedSessions.Add(_currentSession);
            StartNewSession();
        }
    }

    private void LoadSavedSessions(String directory)
    {
        try
        {
            var sessionFiles = Directory.GetFiles(directory, "autodump_session_*.json");
            foreach (var file in sessionFiles)
            {
                try
                {
                    var jsonContent = File.ReadAllText(file);
                    var sessionExport = JsonConvert.DeserializeObject<SessionExport>(jsonContent);
                    if (sessionExport == null) continue;

                    var session = ConvertExportToSession(sessionExport);
                    _completedSessions.Add(session);
                }
                catch (Exception ex)
                {
                    DebugWindow.LogError($"Failed to load session file {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            _completedSessions = _completedSessions.OrderBy(s => s.StartTime).ToList();
        }
        catch (Exception ex)
        {
            DebugWindow.LogError($"Failed to load saved sessions: {ex.Message}");
        }
    }

    private Session ConvertExportToSession(SessionExport export)
    {
        var session = new Session(_gameController)
        {
            StartTime = export.StartTime,
            EndTime = export.EndTime ?? export.StartTime
        };

        foreach (var mapExport in export.Maps)
        {
            var mapRun = new MapRun(mapExport.AreaName, mapExport.AreaHash, _gameController)
            {
                StartTime = mapExport.StartTime
            };

            foreach (var (item, count) in mapExport.ItemDrops)
                mapRun.ItemDrops[item] = count;

            foreach (var (rarityStr, count) in mapExport.MobsByRarity)
            {
                if (Enum.TryParse<MonsterRarity>(rarityStr, out var rarity))
                    mapRun.MobsByRarity[rarity] = count;
            }

            if (mapExport.EndTime.HasValue)
                mapRun.End();

            session.Maps.Add(mapRun);
        }

        return session;
    }

    public string GetCurrentSessionFilePath(string directoryPath)
    {
        if (string.IsNullOrEmpty(_currentSessionFilePath))
        {
            _currentSessionFilePath = Path.Combine(directoryPath,
                $"autodump_session_{_currentSession.StartTime:yyyy-MM-dd_HH-mm-ss}.json");
        }
        return _currentSessionFilePath;
    }

    public Session CurrentSession => _currentSession;
    public IReadOnlyList<Session> CompletedSessions => _completedSessions;
}