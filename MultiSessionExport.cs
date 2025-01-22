using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMetrics;

public class MultiSessionExport
{
    public DateTime ExportDate { get; set; }
    public int TotalSessions { get; set; }
    public List<SessionExport> Sessions { get; set; }

    public static MultiSessionExport FromSessions(IReadOnlyList<Session> sessions)
    {
        return new MultiSessionExport
        {
            ExportDate = DateTime.Now,
            TotalSessions = sessions.Count,
            Sessions = sessions.Select(SessionExport.FromSession).ToList()
        };
    }
}