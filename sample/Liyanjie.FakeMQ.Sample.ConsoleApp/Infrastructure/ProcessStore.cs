using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ.Sample.ConsoleApp.Infrastructure
{
    public class ProcessStore : IFakeMQProcessStore, IDisposable
    {
        readonly SqliteContext db;
        public ProcessStore(SqliteContext db)
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
            return db.FakeMQProcesses.AsNoTracking().SingleOrDefault(_ => _.Subscription == subscription);
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
