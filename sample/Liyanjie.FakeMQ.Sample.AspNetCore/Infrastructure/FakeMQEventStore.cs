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
        readonly SqliteContext context;
        public FakeMQEventStore(SqliteContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }

        public bool Add(FakeMQEvent @event)
        {
            context.FakeMQEvents.Add(@event);
            return Save();
        }

        public FakeMQEvent Get(string type, long timestamp)
        {
            return context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.Timestamp > timestamp)
                .OrderBy(_ => _.Timestamp)
                .FirstOrDefault();
        }

        bool Save()
        {
            context.SaveChanges();
            return true;
        }
    }
}
