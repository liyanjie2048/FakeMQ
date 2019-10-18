using System;
using System.Data.Entity;
using System.Linq;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class FakeMQProcessStore : IFakeMQProcessStore, IDisposable
    {
        readonly SqlCeContext db;
        public FakeMQProcessStore(SqlCeContext db)
        {
            this.db = db;
        }

        public void Dispose()
        {
            this.db?.Dispose();
        }

        public bool Add(FakeMQProcess process)
        {
            if (db.FakeMQProcesses.Any(_ => _.Subscription == process.Subscription))
                return true;

            db.FakeMQProcesses.Add(process);

            return Save();
        }
        public FakeMQProcess Get(string subscription)
        {
            return db.FakeMQProcesses.AsNoTracking()
                .SingleOrDefault(_ => _.Subscription == subscription);
        }
        public bool Update(string subscription, long timestamp)
        {
            var item = db.FakeMQProcesses.SingleOrDefault(_ => _.Subscription == subscription);
            if (item == null)
                return true;

            item.Timestamp = timestamp;

            return Save();
        }
        public bool Delete(string subscription)
        {
            var item = db.FakeMQProcesses.SingleOrDefault(_ => _.Subscription == subscription);
            if (item == null)
                return true;

            db.FakeMQProcesses.Remove(item);

            return Save();
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
