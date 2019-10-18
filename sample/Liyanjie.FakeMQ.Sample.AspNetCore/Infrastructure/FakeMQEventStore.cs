using System;
using System.Linq;
using System.Threading.Tasks;

using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure
{
    public class FakeMQEventStore : IFakeMQEventStore, IDisposable
    {
        readonly SqliteContext db;
        public FakeMQEventStore(SqliteContext db)
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
            db.SaveChanges();
            return true;
        }
    }
}
