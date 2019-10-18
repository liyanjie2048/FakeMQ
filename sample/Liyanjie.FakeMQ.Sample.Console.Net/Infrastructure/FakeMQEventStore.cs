using System;
using System.Data.Entity;
using System.Linq;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class FakeMQEventStore : IFakeMQEventStore, IDisposable
    {
        readonly SqlCeContext db;
        public FakeMQEventStore(SqlCeContext db)
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
