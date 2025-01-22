using ExileCore2;
using MapMetrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMetrics;
public class SessionManager
{
    private List<Session> _completedSessions = new();
    private Session _currentSession;
    private GameController _gameController;

    public SessionManager(GameController gameController)
    {
        _gameController = gameController;
        StartNewSession();
    }

    public void StartNewSession()
    {
        _currentSession = new Session(_gameController);
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

    public Session CurrentSession => _currentSession;
    public IReadOnlyList<Session> CompletedSessions => _completedSessions;
}