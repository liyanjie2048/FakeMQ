using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure
{
    public class FakeMQProcessStore : IFakeMQProcessStore, IDisposable
    {
        readonly SqliteContext context;
        public FakeMQProcessStore(SqliteContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }

        public bool Add(FakeMQProcess process)
        {
            if (context.FakeMQProcesses.Any(_ => _.Subscription == process.Subscription))
                return true;

            context.FakeMQProcesses.Add(process);

            return Save();
        }
        public FakeMQProcess Get(string subscription)
        {
            return context.FakeMQProcesses.AsNoTracking().SingleOrDefault(_ => _.Subscription == subscription);
        }
        public bool Update(string subscription, long timestamp)
        {
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.Subscription == subscription);
            if (item == null)
                return true;

            item.Timestamp = timestamp;

            return Save();
        }
        public bool Delete(string subscription)
        {
            var item = context.FakeMQProcesses.SingleOrDefault(_ => _.Subscription == subscription);
            if (item == null)
                return true;

            context.FakeMQProcesses.Remove(item);

            return Save();
        }

        bool Save()
        {
            context.SaveChanges();
            return true;
        }
    }
}
