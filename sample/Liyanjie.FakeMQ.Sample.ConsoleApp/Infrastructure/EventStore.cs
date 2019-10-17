using System;
using System.Linq;
using System.Threading.Tasks;

using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ.Sample.ConsoleApp.Infrastructure
{
    public class EventStore : IFakeMQEventStore, IDisposable
    {
        readonly SqliteContext db;
        public EventStore(SqliteContext db)
        {
            this.db = db;
        }

        public void Dispose()
        {
            this.db?.Dispose();
        }

        public bool Add(FakeMQEvent @event)
        {
            db.FakeMQEvents.Add(@event);
            return Save();
        }

        public FakeMQEvent Get(string type, long timestamp)
        {
            return db.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.Timestamp > timestamp)
                .OrderBy(_ => _.Timestamp)
                .FirstOrDefault();
        }

        bool Save()
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch { }
            return false;
        }
    }
}
