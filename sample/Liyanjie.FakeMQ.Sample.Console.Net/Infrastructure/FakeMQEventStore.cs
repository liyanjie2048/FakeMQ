using System;
using System.Data.Entity;
using System.Linq;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class FakeMQEventStore : IFakeMQEventStore, IDisposable
    {
        readonly SqlCeContext context;
        public FakeMQEventStore(SqlCeContext context)
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
            try
            {
                context.SaveChanges();
                return true;
            }
            catch { }
            return false;
        }
    }
}
