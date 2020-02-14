using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database;
using Native.Sdk.Cqp;
using Native.Sdk.Cqp.Model;
using SQLite;

namespace Code.Service
{
    public class CoolQDataBase
    {
        private static SQLiteConnection DatabaseEvent { get; set; }
        private static SQLiteConnection DatabaseLog { get; set; }
        private static SQLiteConnection DatabaseCache { get; set; }
        private readonly long QqId;
        public CoolQDataBase(long qqId)
        {
            QqId = qqId;
            string db_CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{QqId}", "cache.db");
            string db_LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{QqId}", "logv2_" + DateTime.Today.Year + DateTime.Today.Month.ToString("d2") + ".db");
            string db_EventPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{QqId}", "eventv2.db");
            DatabaseCache = new SQLiteConnection(db_CachePath);
            DatabaseLog = new SQLiteConnection(db_LogPath);
            DatabaseEvent = new SQLiteConnection(db_EventPath);
        }

        public void NextMonth()
        {
            string db_LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", $"{QqId}", "logv2_" + DateTime.Today.Year + DateTime.Today.Month.ToString("d2") + ".db");
            DatabaseLog.Close();
            DatabaseLog = new SQLiteConnection(db_LogPath);
        }

        public List<CQDBEvent> GetGroupEvent(long groupId, int limit)
        {
            //这个TableQuery不支持这种String组合,所以只好预先处理
            string group = "qq/group/" + groupId;
            return DatabaseEvent.Table<CQDBEvent>().Where(w => w.Group == group && w.Type == 2).OrderByDescending(o => o.Id).Take(limit).ToList();
        }

    }
}
