using ExileCore2;
using MapMetrics;
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

    public SessionManager(GameController gameController)
    {
        _gameController = gameController;
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